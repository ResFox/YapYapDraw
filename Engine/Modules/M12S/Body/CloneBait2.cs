using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using YapYapDraw.Engine.Interop;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Enum;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;
using YapYapDraw.Engine.Vfx;
using YapYapDraw.Windows;

namespace YapYapDraw.Modules.M12S.Body;

public class CloneBait2 : ISpecialAction
{
    public enum Strat { EU, JP, Banana }

    public enum Dir8 { N, NE, E, SE, S, SW, W, NW }

    public enum Tether { Nothing = 0, Fan = 367, Stack = 369, Defamation = 368, Boss = 374 }

    public enum SpotStyle { Circle, Pillar }

    // Final-stack split: tanks + melee dps go one way, healers + ranged the other.
    public enum RoleMode { Auto, Melee, Ranged }

    public class Config
    {
        public bool Active;
        public Strat Strat = Strat.EU;
        public RoleMode Role = RoleMode.Auto;
        public int ColorIndex = 4;
        public bool ShowTether = true;
        public bool ShowGrab = true;
        public bool ShowNorth = true;
        public bool ShowNothingGuide = true;
        public SpotStyle SpotStyle = SpotStyle.Circle;

        public bool Preview;
        public Dir8 PreviewDir = Dir8.N;
        public int PreviewPhase = 1;
        public bool PreviewNetherFar;
        public RoleMode PreviewRole = RoleMode.Melee;
    }

    private sealed class Group
    {
        public Dir8[] Dirs = Array.Empty<Dir8>();
        public Tether[] Tethers = Array.Empty<Tether>();
        public Dictionary<Tether, Vector2>[] Phase = new Dictionary<Tether, Vector2>[6];
        public Dictionary<Tether, Vector2>[] PhaseRanged = new Dictionary<Tether, Vector2>[6];
        public Dictionary<Tether, Vector2> Phase3Far = new();
    }

    private sealed class Preset
    {
        public Group A = new();
        public Group B = new();
        public bool DifferentNetherwrath;
        public Dir8 North;
    }

    private const uint PlayerCloneBaseId = 19210u;
    private const uint SnakingKickId = 46375u;
    private const float Alpha = 0.85f;

    private static bool _enableMigrated;

    private static Config C => ModuleConfig.Get<Config>();

    private static void EnsureEnableMigrated()
    {
        if (_enableMigrated) return;
        _enableMigrated = true;
        ModuleConfig.MigrateLegacyActive("Lindblum/Replication 2 (Clones + Bait)", C.Active);
    }

    private static Vector4 SpotColor
    {
        get
        {
            Vector4 c = StratUI.SwatchColor(C.ColorIndex);
            c.W = Alpha;
            return c;
        }
    }

    private static Vector4 GrabColor => new Vector4(0.20f, 0.95f, 0.35f, Alpha);
    private static Vector4 NorthColor => new Vector4(1.00f, 0.82f, 0.10f, Alpha);
    private static Vector4 NothingColor => new Vector4(0.96f, 0.20f, 0.20f, Alpha);

    private uint _phase;
    private bool _netherFar;
    private uint _myCloneId;
    private Dir8? _dir;
    private readonly Dictionary<uint, uint> _cloneTethers = new();

    private StaticVfx _spot;
    private StaticVfx _tether;
    private StaticVfx _grab;
    private uint _grabId;
    private StaticVfx _north;
    private StaticVfx _northDot;
    private StaticVfx _nothing;
    private Vector3? _spotAt;
    private SpotStyle _spotStyle;

    public override string Name => "Replication 2 (Clones + Bait)";
    public override string? ModuleEnableKey => "Lindblum/Replication 2 (Clones + Bait)";

    public override uint Phase => 2u;

    public override bool HasConfig => true;

    public override HashSet<uint> ActionID => new HashSet<uint>
    {
        46307u, 46383u, 46311u, 46315u, 46384u, 47329u, 48733u,
    };

    private static readonly string[] StratNames = { "EU", "JP", "Codex Banana" };
    private static readonly string[] DirNames = { "N", "NE", "E", "SE", "S", "SW", "W", "NW" };
    private static readonly string[] SpotStyleNames = { "Circle", "Pillar" };
    private static readonly string[] RoleNames = { "Auto", "Melee (T/M)", "Ranged (H/R)" };

    // Healers, physical ranged, casters take the "ranged" stack side; everyone else is melee.
    private static readonly HashSet<uint> RangedJobs = new()
    {
        6, 24, 28, 33, 40,   // healers
        5, 23, 31, 38,       // physical ranged
        7, 26, 25, 27, 35, 36, 42, // casters
    };

    private static bool IsRanged()
    {
        return Svc.Objects.LocalPlayer is IPlayerCharacter me && RangedJobs.Contains(me.ClassJob.RowId);
    }

    public override void OnActionCast(ActorCastInfo info)
    {
        switch (info.ActionId)
        {
            case 46307:
                if (_phase == 0) _phase = 1;
                break;
            case 46383:
                _netherFar = true;
                break;
        }
    }

    public override void OnAbilityCast(ActorAbilityInfo info)
    {
        switch (info.ActionId)
        {
            case 46311 when _phase == 1: _phase = 2; break;
            case 46315 when _phase == 2: _phase = 3; break;
            case 46384 when _phase == 3: _phase = 4; break;
            case 47329 when _phase == 4: _phase = 5; break;
            case 48733 when _phase >= 5: _phase++; break;
        }
    }

    public override void OnActorTetherEvent(uint actorId, uint id, ulong targetId)
    {
        if (id is 367u or 368u or 369u or 374u)
            _cloneTethers[actorId] = id;

        IGameObject me = Svc.Objects.LocalPlayer;
        if (me == null) return;
        if ((uint)targetId != me.EntityId) return;
        IGameObject src = actorId.GameObject();
        if (src == null || src.BaseId != PlayerCloneBaseId) return;
        _myCloneId = actorId;
    }

    public override void OnActorTetherCancelEvent(uint actorId)
    {
        _cloneTethers.Remove(actorId);
        if (actorId == _myCloneId) _myCloneId = 0;
    }

    public override void Update()
    {
        if (_dir == null && _myCloneId != 0)
        {
            IGameObject clone = _myCloneId.GameObject();
            if (clone != null) _dir = DirFromPos(clone.Position);
        }

        Dir8? dir = C.Preview ? C.PreviewDir : _dir;
        EnsureEnableMigrated();
        if (!C.Preview && !ModuleConfig.IsEnabled(ModuleEnableKey))
        {
            Clear();
            return;
        }
        if (dir == null)
        {
            Clear();
            return;
        }

        // Phase 0: position the boss (north) and show which clone to grab from.
        if (!C.Preview && _phase == 0)
        {
            RemoveSpot();
            UpdateNorth(Presets[C.Strat].North);
            UpdateGrab(dir.Value);
            return;
        }

        UpdateGrab(Dir8.N, hide: true);
        if (C.Preview)
            UpdateNorth(Presets[C.Strat].North);
        else
            RemoveNorth();

        int phase = C.Preview ? Math.Clamp(C.PreviewPhase, 1, 5) : (int)_phase;
        bool netherFar = C.Preview ? C.PreviewNetherFar : _netherFar;
        if (phase < 1 || phase > 5)
        {
            Clear();
            return;
        }

        (int lp, Tether tether)? r = Resolve(dir.Value);
        if (r == null)
        {
            RemoveSpot();
            return;
        }

        bool ranged = C.Preview
            ? C.PreviewRole == RoleMode.Ranged
            : (C.Role == RoleMode.Ranged || (C.Role == RoleMode.Auto && IsRanged()));
        Vector3? target = SpotFor(phase, dir.Value, netherFar, r.Value, ranged);
        if (phase == 3 && !C.Preview)
        {
            Vector3? snake = SnakingKickSafe();
            if (snake != null) target = snake;
        }

        if (target == null)
        {
            RemoveSpot();
            return;
        }

        if (_spot == null || _spotAt == null || _spotStyle != C.SpotStyle || Vector3.Distance(_spotAt.Value, target.Value) > 0.05f)
        {
            RemoveSpot();
            _spot = SpawnSpot(target.Value);
            _spotAt = target.Value;
            _spotStyle = C.SpotStyle;
            if (_spot != null) aoes.Add(_spot);
        }

        if (C.ShowTether && _tether == null)
        {
            _tether = SpawnTether(target.Value);
            if (_tether != null) aoes.Add(_tether);
        }
        else if (!C.ShowTether && _tether != null)
        {
            _tether.Remove();
            aoes.Remove(_tether);
            _tether = null;
        }
    }

    private (int lp, Tether tether)? Resolve(Dir8 dir)
    {
        Preset p = Presets[C.Strat];
        for (int i = 0; i < p.A.Dirs.Length; i++)
            if (p.A.Dirs[i] == dir) return (0, p.A.Tethers[i]);
        for (int i = 0; i < p.B.Dirs.Length; i++)
            if (p.B.Dirs[i] == dir) return (1, p.B.Tethers[i]);
        return null;
    }

    private Vector3? SpotFor(int phase, Dir8 dir, bool netherFar, (int lp, Tether tether) r, bool ranged)
    {
        if (phase < 1 || phase > 5) return null;
        Preset p = Presets[C.Strat];
        Group grp = r.lp == 0 ? p.A : p.B;
        Dictionary<Tether, Vector2> dict;
        if (phase == 3 && netherFar && p.DifferentNetherwrath)
            dict = grp.Phase3Far;
        else if (ranged && grp.PhaseRanged[phase] != null)
            dict = grp.PhaseRanged[phase];
        else
            dict = grp.Phase[phase];
        if (dict != null && dict.TryGetValue(r.tether, out Vector2 v))
            return new Vector3(v.X, 0f, v.Y);
        return null;
    }

    private void UpdateNorth(Dir8 north)
    {
        if (!C.ShowNorth)
        {
            RemoveNorth();
            return;
        }
        if (_north != null) return;
        float rad = (int)north * 45f * (MathF.PI / 180f);
        var edge = new Vector3(100f + 20f * MathF.Sin(rad), 0f, 100f - 20f * MathF.Cos(rad));
        _north = DrawManager.Draw(new DrawElement
        {
            drawAvfx = "customRect",
            radiusX = 0.5f,
            radiusY = 1f,
            radiusZ = 1f,
            drawOnObject = false,
            Position = new Vector3(100f, 0f, 100f),
            endToTarget = true,
            targetPosition = edge,
            refColor = NorthColor,
            refTargetColor = NorthColor,
            destroyTime = 600000f,
        });
        if (_north != null) aoes.Add(_north);
        _northDot = DrawManager.Draw(new DrawElement
        {
            drawAvfx = "customCircle",
            radiusX = 2.5f,
            radiusZ = 2.5f,
            drawOnObject = false,
            Position = edge,
            refColor = NorthColor,
            refTargetColor = NorthColor,
            destroyTime = 600000f,
        });
        if (_northDot != null) aoes.Add(_northDot);
    }

    private void UpdateGrab(Dir8 dir, bool hide = false)
    {
        if (hide || !C.ShowGrab)
        {
            RemoveGrab();
            RemoveNothing();
            return;
        }

        (int lp, Tether tether)? r = Resolve(dir);
        if (r == null)
        {
            RemoveGrab();
            RemoveNothing();
            return;
        }

        if (r.Value.tether == Tether.Nothing)
        {
            RemoveGrab();
            UpdateNothing();
            return;
        }

        RemoveNothing();
        uint targetId = FindGrabTarget(r.Value);
        if (targetId == 0)
        {
            RemoveGrab();
            return;
        }
        if (_grabId == targetId && _grab != null) return;
        RemoveGrab();
        IGameObject obj = targetId.GameObject();
        if (obj == null) return;
        _grab = DrawManager.Draw(new DrawElement
        {
            drawAvfx = "customCircle",
            radiusX = 3.5f,
            radiusZ = 3.5f,
            radiusY = 3f,
            drawOnObject = true,
            refColor = GrabColor,
            refTargetColor = GrabColor,
            destroyTime = 600000f,
        }, obj);
        _grabId = targetId;
        if (_grab != null) aoes.Add(_grab);
    }

    private uint FindGrabTarget((int lp, Tether tether) r)
    {
        uint want = (uint)r.tether;
        float start = (int)Presets[C.Strat].North * 45f - 5f;
        var list = new List<(uint id, float key)>();
        foreach (KeyValuePair<uint, uint> kv in _cloneTethers)
        {
            if (kv.Value != want) continue;
            IGameObject o = kv.Key.GameObject();
            if (o == null) continue;
            float a = AngleCw(o.Position);
            float key = (((a - start) % 360f) + 360f) % 360f;
            list.Add((kv.Key, key));
        }
        if (list.Count == 0) return 0;
        list.Sort((x, y) => x.key.CompareTo(y.key));
        return r.lp == 0 ? list[0].id : list[^1].id;
    }

    private void UpdateNothing()
    {
        if (!C.ShowNothingGuide || _myCloneId == 0)
        {
            RemoveNothing();
            return;
        }
        if (_nothing != null) return;
        IGameObject clone = _myCloneId.GameObject();
        if (clone == null) return;
        _nothing = DrawManager.Draw(new DrawElement
        {
            drawAvfx = "customCircle",
            radiusX = 2f,
            radiusZ = 2f,
            drawOnObject = true,
            refColor = NothingColor,
            refTargetColor = NothingColor,
            destroyTime = 600000f,
        }, clone);
        if (_nothing != null) aoes.Add(_nothing);
    }

    private static Vector3? SnakingKickSafe()
    {
        foreach (IGameObject obj in YapYapDraw.Plugin.ObjectTable)
        {
            try
            {
                if (obj is not IBattleChara bc || !bc.IsValid()) continue;
                if (!bc.IsCasting || bc.CastActionId != SnakingKickId) continue;
                float a = bc.Rotation + MathF.PI;
                return new Vector3(
                    bc.Position.X + 3f * MathF.Sin(a),
                    0f,
                    bc.Position.Z + 3f * MathF.Cos(a));
            }
            catch
            {
            }
        }
        return null;
    }

    private static float AngleCw(Vector3 p)
    {
        float ang = MathF.Atan2(p.X - 100f, -(p.Z - 100f)) * (180f / MathF.PI);
        if (ang < 0f) ang += 360f;
        return ang;
    }

    private static Dir8? DirFromPos(Vector3 p)
    {
        float dx = p.X - 100f;
        float dz = p.Z - 100f;
        if (MathF.Sqrt(dx * dx + dz * dz) < 2f) return null;
        float slot = AngleCw(p) / 45f;
        int idx = (int)MathF.Round(slot) % 8;
        if (MathF.Abs(slot - MathF.Round(slot)) > 0.25f) return null;
        return (Dir8)idx;
    }

    private static StaticVfx SpawnSpot(Vector3 pos)
    {
        bool pillar = C.SpotStyle == SpotStyle.Pillar;
        return DrawManager.Draw(new DrawElement
        {
            drawAvfx = pillar ? GroundOmen.WhiteTower : "customCircle",
            radiusX = 1.3f,
            radiusZ = 1.3f,
            radiusY = 1f,
            drawOnObject = false,
            Position = pos,
            refColor = SpotColor,
            refTargetColor = SpotColor,
            destroyTime = 600000f,
        });
    }

    private static StaticVfx SpawnTether(Vector3 pos)
    {
        IGameObject me = Svc.Objects.LocalPlayer;
        if (me == null) return null;
        return DrawManager.Draw(new DrawElement
        {
            drawAvfx = "customRect",
            radiusX = 0.35f,
            radiusY = 1f,
            radiusZ = 1f,
            drawOnObject = true,
            endToTarget = true,
            targetPosition = pos,
            refColor = SpotColor,
            refTargetColor = SpotColor,
            destroyTime = 600000f,
        }, me);
    }

    private void RemoveSpot()
    {
        if (_spot != null)
        {
            _spot.Remove();
            aoes.Remove(_spot);
            _spot = null;
        }
        if (_tether != null)
        {
            _tether.Remove();
            aoes.Remove(_tether);
            _tether = null;
        }
        _spotAt = null;
    }

    private void RemoveGrab()
    {
        if (_grab != null)
        {
            _grab.Remove();
            aoes.Remove(_grab);
            _grab = null;
        }
        _grabId = 0;
    }

    private void RemoveNorth()
    {
        if (_north != null)
        {
            _north.Remove();
            aoes.Remove(_north);
            _north = null;
        }
        if (_northDot != null)
        {
            _northDot.Remove();
            aoes.Remove(_northDot);
            _northDot = null;
        }
    }

    private void RemoveNothing()
    {
        if (_nothing != null)
        {
            _nothing.Remove();
            aoes.Remove(_nothing);
            _nothing = null;
        }
    }

    private void Clear()
    {
        RemoveSpot();
        RemoveGrab();
        RemoveNorth();
        RemoveNothing();
    }

    public override void Reset()
    {
        base.Reset();
        Clear();
        _cloneTethers.Clear();
        _phase = 0;
        _netherFar = false;
        _myCloneId = 0;
        _dir = null;
    }

    public override void DrawConfig()
    {
        EnsureEnableMigrated();
        bool active = ModuleConfig.IsEnabled(ModuleEnableKey);
        if (StratUI.Header("Replication 2 \u2014 Clones", ref active))
        {
            ModuleConfig.SetEnabled(ModuleEnableKey, active);
            C.Active = active;
            ModuleConfig.Save<Config>();
        }

        StratUI.Section("Strategy");
        int strat = (int)C.Strat;
        if (StratUI.SegmentedBar(StratNames, ref strat))
        {
            C.Strat = (Strat)strat;
            ModuleConfig.Save<Config>();
        }
        StratUI.Hint($"Relative north for this strat: {Presets[C.Strat].North}. Side is read live from your clone tether.");

        StratUI.Section("Role group (final stacks)");
        int role = (int)C.Role;
        if (StratUI.SegmentedBar(RoleNames, ref role))
        {
            C.Role = (RoleMode)role;
            ModuleConfig.Save<Config>();
        }
        StratUI.Hint(C.Role == RoleMode.Auto
            ? $"Auto from your job \u2014 currently {(IsRanged() ? "Ranged (H/R)" : "Melee (T/M)")}. Tanks + melee stack one side, healers + ranged the other."
            : "Tanks + melee stack one side; healers + ranged the other. Only changes the last two stacks.");

        StratUI.Section("Marker color");
        int color = C.ColorIndex;
        if (StratUI.ColorSwatches(ref color))
        {
            C.ColorIndex = color;
            ModuleConfig.Save<Config>();
        }
        ImGui.SameLine();
        ImGui.AlignTextToFramePadding();
        ImGui.TextDisabled(StratUI.SwatchName(C.ColorIndex));

        StratUI.Section("Spot style");
        int spotStyle = (int)C.SpotStyle;
        if (StratUI.SegmentedBar(SpotStyleNames, ref spotStyle))
        {
            C.SpotStyle = (SpotStyle)spotStyle;
            ModuleConfig.Save<Config>();
        }
        StratUI.Hint("Pillar is a tall beam of light \u2014 stays visible above other ground AoEs.");

        StratUI.Section("Show");
        bool grab = C.ShowGrab;
        if (ImGui.Checkbox("Clone to grab the tether from (green)", ref grab)) { C.ShowGrab = grab; ModuleConfig.Save<Config>(); }
        bool north = C.ShowNorth;
        if (ImGui.Checkbox("Strat north line (yellow)", ref north)) { C.ShowNorth = north; ModuleConfig.Save<Config>(); }
        bool tether = C.ShowTether;
        if (ImGui.Checkbox("Guide line to my spot", ref tether)) { C.ShowTether = tether; ModuleConfig.Save<Config>(); }
        bool nothing = C.ShowNothingGuide;
        if (ImGui.Checkbox("Mark my clone when I take no tether", ref nothing)) { C.ShowNothingGuide = nothing; ModuleConfig.Save<Config>(); }

        StratUI.Section("Preview");
        bool preview = C.Preview;
        if (ImGui.Checkbox("Preview in arena (ignores phase, for testing)", ref preview))
        {
            C.Preview = preview;
            ModuleConfig.Save<Config>();
        }
        if (C.Preview)
        {
            ImGui.AlignTextToFramePadding();
            ImGui.TextDisabled("Your clone direction:");
            int pdir = (int)C.PreviewDir;
            if (StratUI.SegmentedBar(DirNames, ref pdir))
            {
                C.PreviewDir = (Dir8)pdir;
                ModuleConfig.Save<Config>();
            }
            int pphase = C.PreviewPhase;
            ImGui.SetNextItemWidth(220f);
            if (ImGui.SliderInt("Phase", ref pphase, 1, 5))
            {
                C.PreviewPhase = Math.Clamp(pphase, 1, 5);
                ModuleConfig.Save<Config>();
            }
            ImGui.AlignTextToFramePadding();
            ImGui.TextDisabled("Role group:");
            int prole = C.PreviewRole == RoleMode.Ranged ? 1 : 0;
            if (StratUI.SegmentedBar(new[] { "Melee (T/M)", "Ranged (H/R)" }, ref prole))
            {
                C.PreviewRole = prole == 1 ? RoleMode.Ranged : RoleMode.Melee;
                ModuleConfig.Save<Config>();
            }
            if (Presets[C.Strat].DifferentNetherwrath)
            {
                bool nf = C.PreviewNetherFar;
                if (ImGui.Checkbox("Netherwrath far (phase 3)", ref nf))
                {
                    C.PreviewNetherFar = nf;
                    ModuleConfig.Save<Config>();
                }
            }
            (int lp, Tether tether)? rp = Resolve(C.PreviewDir);
            if (rp == null)
            {
                ImGui.TextDisabled("This direction is unused by the selected strat.");
            }
            else
            {
                Vector3? sp = SpotFor(Math.Clamp(C.PreviewPhase, 1, 5), C.PreviewDir, C.PreviewNetherFar, rp.Value, C.PreviewRole == RoleMode.Ranged);
                string grp = rp.Value.lp == 0 ? "Group 1" : "Group 2";
                string coord = sp == null ? "(none)" : $"({sp.Value.X:0.0}, {sp.Value.Z:0.0})";
                ImGui.TextDisabled($"{grp} \u00b7 tether {rp.Value.tether} \u00b7 spot {coord}");
            }
        }

        if (ImGui.CollapsingHeader("Debug"))
        {
            ImGui.TextUnformatted($"Phase {_phase}   Netherwrath far {_netherFar}");
            ImGui.TextUnformatted($"My clone id {_myCloneId}   Direction {(_dir?.ToString() ?? "?")}");
            ImGui.TextUnformatted($"Tracked clone tethers: {_cloneTethers.Count}");
            if (_dir != null)
            {
                (int lp, Tether tether)? rr = Resolve(_dir.Value);
                if (rr != null)
                    ImGui.TextUnformatted($"Group {(rr.Value.lp == 0 ? 1 : 2)}  tether {rr.Value.tether}");
            }
        }
    }

    private static Vector2 V(float x, float y) => new(x, y);

    private static Dictionary<Tether, Vector2> D(
        (Tether t, Vector2 p) a, (Tether t, Vector2 p) b, (Tether t, Vector2 p) c, (Tether t, Vector2 p) d)
        => new() { [a.t] = a.p, [b.t] = b.p, [c.t] = c.p, [d.t] = d.p };

    private static readonly Dictionary<Strat, Preset> Presets = BuildPresets();

    private static Dictionary<Strat, Preset> BuildPresets()
    {
        var presets = new Dictionary<Strat, Preset>();

        // EU (melee in Phase[], healers/ranged stack the opposite side on phases 4-5)
        {
            var p = new Preset { DifferentNetherwrath = true, North = Dir8.N };
            p.A.Dirs = new[] { Dir8.N, Dir8.NE, Dir8.E, Dir8.SE };
            p.A.Tethers = new[] { Tether.Boss, Tether.Fan, Tether.Stack, Tether.Defamation };
            p.A.Phase[1] = D((Tether.Stack, V(107.196f, 82.289f)), (Tether.Fan, V(102.328f, 81.136f)), (Tether.Defamation, V(118.145f, 107.307f)), (Tether.Boss, V(99.918f, 89.278f)));
            p.A.Phase[2] = D((Tether.Stack, V(105.4f, 94.6f)), (Tether.Fan, V(102.8f, 92f)), (Tether.Defamation, V(105.4f, 94.6f)), (Tether.Boss, V(105.4f, 94.6f)));
            p.A.Phase[3] = D((Tether.Stack, V(107.148f, 82.253f)), (Tether.Fan, V(102.374f, 81.150f)), (Tether.Defamation, V(104.5f, 91f)), (Tether.Boss, V(108f, 91f)));
            p.A.Phase3Far = D((Tether.Stack, V(107.148f, 82.253f)), (Tether.Fan, V(102.374f, 81.150f)), (Tether.Defamation, V(113.650f, 89.192f)), (Tether.Boss, V(110.264f, 89.069f)));
            p.A.Phase[4] = D((Tether.Boss, V(113.5f, 100f)), (Tether.Fan, V(113.5f, 100f)), (Tether.Defamation, V(113.5f, 100f)), (Tether.Stack, V(113.5f, 100f)));
            p.A.Phase[5] = D((Tether.Fan, V(110f, 87.5f)), (Tether.Defamation, V(110f, 87.5f)), (Tether.Stack, V(110f, 87.5f)), (Tether.Boss, V(110f, 87.5f)));
            p.A.PhaseRanged[4] = D((Tether.Boss, V(86.5f, 100f)), (Tether.Fan, V(86.5f, 100f)), (Tether.Defamation, V(86.5f, 100f)), (Tether.Stack, V(86.5f, 100f)));
            p.A.PhaseRanged[5] = D((Tether.Fan, V(90f, 87.5f)), (Tether.Defamation, V(90f, 87.5f)), (Tether.Stack, V(90f, 87.5f)), (Tether.Boss, V(90f, 87.5f)));
            p.B.Dirs = new[] { Dir8.NW, Dir8.W, Dir8.SW, Dir8.S };
            p.B.Tethers = new[] { Tether.Fan, Tether.Stack, Tether.Defamation, Tether.Nothing };
            p.B.Phase[1] = D((Tether.Stack, V(92.600f, 82.088f)), (Tether.Fan, V(97.359f, 81.348f)), (Tether.Defamation, V(82.499f, 108.390f)), (Tether.Nothing, V(100.337f, 119.414f)));
            p.B.Phase[2] = D((Tether.Stack, V(94.5f, 94.6f)), (Tether.Fan, V(96.8f, 92f)), (Tether.Defamation, V(94.5f, 94.6f)), (Tether.Nothing, V(94.5f, 94.6f)));
            p.B.Phase[3] = D((Tether.Stack, V(92.572f, 82.140f)), (Tether.Fan, V(97.499f, 81.462f)), (Tether.Defamation, V(95.5f, 91f)), (Tether.Nothing, V(92f, 91f)));
            p.B.Phase3Far = D((Tether.Stack, V(92.572f, 82.140f)), (Tether.Fan, V(97.499f, 81.462f)), (Tether.Defamation, V(86.323f, 89.144f)), (Tether.Nothing, V(89.569f, 89.074f)));
            p.B.Phase[4] = D((Tether.Nothing, V(113.5f, 100f)), (Tether.Fan, V(113.5f, 100f)), (Tether.Defamation, V(113.5f, 100f)), (Tether.Stack, V(113.5f, 100f)));
            p.B.Phase[5] = D((Tether.Nothing, V(110f, 87.5f)), (Tether.Fan, V(110f, 87.5f)), (Tether.Defamation, V(110f, 87.5f)), (Tether.Stack, V(110f, 87.5f)));
            p.B.PhaseRanged[4] = D((Tether.Nothing, V(86.5f, 100f)), (Tether.Fan, V(86.5f, 100f)), (Tether.Defamation, V(86.5f, 100f)), (Tether.Stack, V(86.5f, 100f)));
            p.B.PhaseRanged[5] = D((Tether.Nothing, V(90f, 87.5f)), (Tether.Fan, V(90f, 87.5f)), (Tether.Defamation, V(90f, 87.5f)), (Tether.Stack, V(90f, 87.5f)));
            presets[Strat.EU] = p;
        }

        // JP
        {
            var p = new Preset { DifferentNetherwrath = false, North = Dir8.E };
            p.A.Dirs = new[] { Dir8.SW, Dir8.S, Dir8.SE, Dir8.E };
            p.A.Tethers = new[] { Tether.Defamation, Tether.Fan, Tether.Stack, Tether.Boss };
            p.A.Phase[1] = D((Tether.Stack, V(119f, 104f)), (Tether.Fan, V(117.5f, 108f)), (Tether.Defamation, V(100f, 119.5f)), (Tether.Boss, V(113.75f, 100f)));
            p.A.Phase[2] = D((Tether.Stack, V(106f, 104.5f)), (Tether.Fan, V(108.5f, 102.5f)), (Tether.Defamation, V(106f, 104.5f)), (Tether.Boss, V(106f, 104.5f)));
            p.A.Phase[3] = D((Tether.Stack, V(115.8f, 102.5f)), (Tether.Fan, V(117.5f, 105.5f)), (Tether.Defamation, V(108f, 99.5f)), (Tether.Boss, V(108f, 99.5f)));
            p.A.Phase[4] = D((Tether.Stack, V(119f, 104f)), (Tether.Fan, V(117.5f, 108f)), (Tether.Defamation, V(110f, 90f)), (Tether.Boss, V(110f, 90f)));
            p.A.Phase[5] = D((Tether.Stack, V(119f, 104f)), (Tether.Fan, V(117.5f, 108f)), (Tether.Defamation, V(110f, 110f)), (Tether.Boss, V(110f, 110f)));
            p.B.Dirs = new[] { Dir8.W, Dir8.NW, Dir8.N, Dir8.NE };
            p.B.Tethers = new[] { Tether.Nothing, Tether.Defamation, Tether.Fan, Tether.Stack };
            p.B.Phase[1] = D((Tether.Stack, V(119f, 97.5f)), (Tether.Fan, V(118f, 93f)), (Tether.Defamation, V(100f, 80.5f)), (Tether.Nothing, V(80.5f, 100f)));
            p.B.Phase[2] = D((Tether.Stack, V(106f, 95.5f)), (Tether.Fan, V(108.5f, 97.5f)), (Tether.Defamation, V(106f, 95.5f)), (Tether.Nothing, V(106f, 95.5f)));
            p.B.Phase[3] = D((Tether.Stack, V(115.8f, 97.5f)), (Tether.Fan, V(117.5f, 94.5f)), (Tether.Defamation, V(108f, 99.5f)), (Tether.Nothing, V(108f, 99.5f)));
            p.B.Phase[4] = D((Tether.Stack, V(119f, 97.5f)), (Tether.Fan, V(118f, 93f)), (Tether.Defamation, V(110f, 90f)), (Tether.Nothing, V(110f, 90f)));
            p.B.Phase[5] = D((Tether.Stack, V(119f, 97.5f)), (Tether.Fan, V(118f, 93f)), (Tether.Defamation, V(110f, 110f)), (Tether.Nothing, V(110f, 110f)));
            presets[Strat.JP] = p;
        }

        // Codex Banana
        {
            var p = new Preset { DifferentNetherwrath = false, North = Dir8.W };
            p.A.Dirs = new[] { Dir8.W, Dir8.NW, Dir8.N, Dir8.NE };
            p.A.Tethers = new[] { Tether.Boss, Tether.Stack, Tether.Fan, Tether.Defamation };
            p.A.Phase[1] = D((Tether.Stack, V(81f, 96f)), (Tether.Fan, V(82.5f, 92f)), (Tether.Defamation, V(100f, 80.5f)), (Tether.Boss, V(89f, 100f)));
            p.A.Phase[2] = D((Tether.Stack, V(94f, 94.5f)), (Tether.Fan, V(91.5f, 97.5f)), (Tether.Defamation, V(94f, 94.5f)), (Tether.Boss, V(94f, 104.5f)));
            p.A.Phase[3] = D((Tether.Stack, V(89f, 96.5f)), (Tether.Fan, V(89f, 91f)), (Tether.Defamation, V(82.5f, 100.5f)), (Tether.Boss, V(82.5f, 100.5f)));
            p.A.Phase[4] = D((Tether.Stack, V(81f, 96f)), (Tether.Fan, V(82.5f, 92f)), (Tether.Defamation, V(90f, 110f)), (Tether.Boss, V(90f, 110f)));
            p.A.Phase[5] = D((Tether.Stack, V(81f, 96f)), (Tether.Fan, V(82.5f, 92f)), (Tether.Defamation, V(90f, 90f)), (Tether.Boss, V(90f, 90f)));
            p.B.Dirs = new[] { Dir8.SW, Dir8.S, Dir8.SE, Dir8.E };
            p.B.Tethers = new[] { Tether.Stack, Tether.Fan, Tether.Defamation, Tether.Nothing };
            p.B.Phase[1] = D((Tether.Stack, V(81f, 102.5f)), (Tether.Fan, V(82f, 107f)), (Tether.Defamation, V(100f, 119.5f)), (Tether.Nothing, V(119.5f, 100f)));
            p.B.Phase[2] = D((Tether.Stack, V(94f, 104.5f)), (Tether.Fan, V(91.5f, 102.5f)), (Tether.Defamation, V(94f, 104.5f)), (Tether.Nothing, V(94f, 94.5f)));
            p.B.Phase[3] = D((Tether.Stack, V(89f, 103.5f)), (Tether.Fan, V(89f, 109f)), (Tether.Defamation, V(82.5f, 100.5f)), (Tether.Nothing, V(82.5f, 100.5f)));
            p.B.Phase[4] = D((Tether.Stack, V(81f, 102.5f)), (Tether.Fan, V(82f, 107f)), (Tether.Defamation, V(90f, 110f)), (Tether.Nothing, V(90f, 110f)));
            p.B.Phase[5] = D((Tether.Stack, V(81f, 102.5f)), (Tether.Fan, V(82f, 107f)), (Tether.Defamation, V(90f, 90f)), (Tether.Nothing, V(90f, 90f)));
            presets[Strat.Banana] = p;
        }

        return presets;
    }
}
