using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.ClientState.Statuses;
using YapYapDraw.Engine.Interop;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Enum;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.QuickDraws;
using YapYapDraw.Engine.Struct;
using YapYapDraw.Engine.Util;
using YapYapDraw.Engine.Vfx;
using YapYapDraw.Windows;
using YapYapDraw.Logging;

namespace YapYapDraw.Modules.M12S.Body;

public class CloneHighlight : ISpecialAction
{
    public enum Strat { StaticEU, DN, Relative }

    public enum Role { MT, OT, M1, M2, H1, H2, R1, R2 }

    private enum RoleType { Tank, Melee, Healer, Ranged }

    public enum Dir { None, NE, SE, SW, NW }

    public class Config
    {
        public bool Active;
        public Strat Strat = Strat.StaticEU;
        public Role Role = Role.MT;
        public int ColorIndex = 4;

        public bool RelDarkLeft = true;
        public bool RelFireLeft = true;
        public bool ShowTether = true;

        public bool Preview;
        public Dir PreviewDir = Dir.NW;
        public bool PreviewFire;
    }

    private const uint CloneBaseId = 19204u;
    private const uint DarkResistanceDown = 3323u;

    private static bool _enableMigrated;

    private static Config C => ModuleConfig.Get<Config>();

    private static void EnsureEnableMigrated()
    {
        if (_enableMigrated) return;
        _enableMigrated = true;
        ModuleConfig.MigrateLegacyActive("Lindblum/Replication 1 (Clones + Bait)", C.Active);
    }

    private static readonly Dictionary<Dir, Vector2> CloneDirections = new()
    {
        [Dir.NE] = new Vector2(109.88013f, 90.073975f),
        [Dir.SE] = new Vector2(109.88013f, 109.88013f),
        [Dir.SW] = new Vector2(90.073975f, 109.88013f),
        [Dir.NW] = new Vector2(90.073975f, 90.073975f),
    };

    private static Vector4 DarkColor => new Vector4(0.6f, 0f, 1f, GroundOmen.Red.W);

    private static Vector4 FireColor => GroundOmen.Red;

    private static Vector4 BaitColor
    {
        get
        {
            Vector4 c = StratUI.SwatchColor(C.ColorIndex);
            c.W = GroundOmen.Red.W;
            return c;
        }
    }

    private ulong _darkMaster;
    private ulong _fireMaster;
    private readonly List<ulong> _darkClones = new();
    private readonly List<ulong> _fireClones = new();
    private readonly Dictionary<ulong, StaticVfx> _rings = new();

    private uint _phase;
    private Dir _north = Dir.None;
    private StaticVfx _bait;
    private Vector3? _baitAt;
    private const string GuideOwner = "m12s_rep1_guide";
    private bool _guideLive;
    private bool? _lastGuideDark;
    private Vector3? _lastGuideSpot;
    private bool _lastTether;

    public override string Name => "Replication 1 (Clones + Bait)";
    public override string? ModuleEnableKey => "Lindblum/Replication 1 (Clones + Bait)";

    public override uint Phase => 2u;

    public override HashSet<uint> ActionID => new HashSet<uint> { 46303u, 46301u, 46304u, 46368u, 46345u };

    public override bool HasConfig => true;

    private static readonly string[] StratNames = { "Static/EU", "DN/NA", "Clone Relative" };
    private static readonly string[] RoleNames = { "MT", "OT", "M1", "M2", "H1", "H2", "R1", "R2" };
    private static readonly string[] PreviewDirNames = { "NE", "SE", "SW", "NW" };

    public override void DrawConfig()
    {
        EnsureEnableMigrated();
        bool active = ModuleConfig.IsEnabled(ModuleEnableKey);
        if (StratUI.Header("Replication 1 \u2014 Clones", ref active))
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

        StratUI.Section("Your role");
        StratUI.Hint("Left column = Group 1 (baits left) \u00b7 right column = Group 2 (baits right)");
        int role = (int)C.Role;
        if (StratUI.RoleGrid(RoleNames, ref role, 2))
        {
            C.Role = (Role)role;
            ModuleConfig.Save<Config>();
        }

        if (C.Strat == Strat.StaticEU)
        {
            StratUI.Hint("Side is taken from your group automatically.");
        }
        else if (C.Strat == Strat.Relative)
        {
            ImGui.Spacing();
            bool dl = C.RelDarkLeft;
            if (ImGui.Checkbox("Dark: bait on LEFT of your clone", ref dl))
            {
                C.RelDarkLeft = dl;
                ModuleConfig.Save<Config>();
            }
            if (!IsMelee(C.Role))
            {
                bool fl = C.RelFireLeft;
                if (ImGui.Checkbox("Fire: bait on LEFT of your clone", ref fl))
                {
                    C.RelFireLeft = fl;
                    ModuleConfig.Save<Config>();
                }
            }
        }

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
        bool tether = C.ShowTether;
        if (ImGui.Checkbox("Show guide line to the spot", ref tether))
        {
            C.ShowTether = tether;
            ModuleConfig.Save<Config>();
        }

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
            ImGui.TextDisabled("Relative north:");
            ImGui.SameLine();
            int pdir = (int)C.PreviewDir - 1;
            if (pdir < 0) pdir = 3;
            if (StratUI.SegmentedBar(PreviewDirNames, ref pdir))
            {
                C.PreviewDir = (Dir)(pdir + 1);
                ModuleConfig.Save<Config>();
            }
            bool pf = C.PreviewFire;
            if (ImGui.Checkbox("You have Dark Resistance Down (bait fire)", ref pf))
            {
                C.PreviewFire = pf;
                ModuleConfig.Save<Config>();
            }
            Vector2 pv = ComputeBait(C.PreviewDir, C.PreviewFire);
            ImGui.TextDisabled($"Bait coord ({pv.X:0.0}, {pv.Y:0.0})  \u00b7  radius {BaitRadius():0.00}");
        }

        if (ImGui.CollapsingHeader("Debug"))
        {
            ImGui.TextUnformatted($"Phase {_phase}   Relative north {_north}");
            ImGui.TextUnformatted($"Dark clones {_darkClones.Count}   Fire clones {_fireClones.Count}");
            ImGui.TextUnformatted($"Dark Resistance Down: {HasDarkResDown()}");
            ImGui.TextUnformatted($"Role {C.Role}: melee={IsMelee(C.Role)}, group1/left={IsGroup1(C.Role)}, type={RoleOf(C.Role)}");
            ImGui.Separator();
            foreach (Dir d in new[] { Dir.NE, Dir.SE, Dir.SW, Dir.NW })
            {
                Vector2 dk = ComputeBait(d, false);
                Vector2 fr = ComputeBait(d, true);
                ImGui.TextUnformatted($"{d}: dark ({dk.X:0.0},{dk.Y:0.0})  fire ({fr.X:0.0},{fr.Y:0.0})");
            }
        }
    }

    public override void Reset()
    {
        base.Reset();
        ClearAll();
    }

    public override void OnActionCast(ActorCastInfo info)
    {
        switch (info.ActionId)
        {
            case 46345:
            case 46368:
                ClearAll();
                break;
            case 46303:
                if (_darkMaster == 0) _darkMaster = info.SourceId;
                break;
            case 46301:
                if (_fireMaster == 0) _fireMaster = info.SourceId;
                break;
        }
    }

    public override void OnAbilityCast(ActorAbilityInfo info)
    {
        if (info.ActionId == 46345)
        {
            ClearAll();
            return;
        }
        if (info.ActionId == 46304 && _phase == 0 && _darkClones.Count == 2)
        {
            _phase = 1;
        }
    }

    public override void Update()
    {
        EnsureEnableMigrated();
        Dictionary<ulong, Vector4> desired = new();
        Resolve(_darkMaster, _darkClones, DarkColor, desired);
        Resolve(_fireMaster, _fireClones, FireColor, desired);

        foreach (ulong id in _rings.Keys.ToList())
        {
            if (!desired.ContainsKey(id))
            {
                _rings[id]?.Remove();
                aoes.Remove(_rings[id]);
                _rings.Remove(id);
            }
        }

        foreach (KeyValuePair<ulong, Vector4> want in desired)
        {
            if (_rings.TryGetValue(want.Key, out StaticVfx existing))
            {
                if (existing != null) existing.Enable = true;
                continue;
            }
            IGameObject clone = want.Key.GameObject();
            if (clone == null) continue;
            StaticVfx vfx = Spawn(clone, want.Value);
            if (vfx != null)
            {
                _rings[want.Key] = vfx;
                aoes.Add(vfx);
            }
        }

        UpdateBait();
    }

    private void UpdateBait()
    {
        if (IdyllicDream.IsRunning)
        {
            RemoveBait();
            ClearGuide();
            return;
        }

        if (_north == Dir.None && _darkClones.Count == 2)
        {
            _north = FindNorth();
        }

        Vector3? target = ResolveBaitSpot();
        if (target == null)
        {
            RemoveBait();
            ClearGuide();
            return;
        }

        bool darkDebuff = C.Preview ? C.PreviewFire : HasDarkResDown();
        RefreshGuide(darkDebuff, target.Value);

        if (_bait == null || _baitAt == null || Vector3.Distance(_baitAt.Value, target.Value) > 0.05f)
        {
            RemoveBait();
            _bait = SpawnBait(target.Value, BaitRadius());
            _baitAt = target.Value;
            if (_bait != null) aoes.Add(_bait);
        }
    }

    private Vector3? ResolveBaitSpot()
    {
        if (C.Preview)
        {
            Vector2 pv = ComputeBait(C.PreviewDir, C.PreviewFire);
            return pv != Vector2.Zero ? new Vector3(pv.X, 0f, pv.Y) : null;
        }
        if (!ModuleConfig.IsEnabled(ModuleEnableKey) || _north == Dir.None)
            return null;
        bool darkDebuff = HasDarkResDown();
        Vector2 p = ComputeBait(_north, darkDebuff);
        return p != Vector2.Zero ? new Vector3(p.X, 0f, p.Y) : null;
    }

    private Dir FindNorth()
    {
        foreach (ulong id in _darkClones)
        {
            IGameObject obj = id.GameObject();
            if (obj == null) continue;
            Vector2 p = new Vector2(obj.Position.X, obj.Position.Z);
            foreach (KeyValuePair<Dir, Vector2> kv in CloneDirections)
            {
                if (Vector2.Distance(kv.Value, p) < 1f)
                {
                    return kv.Key;
                }
            }
        }
        return Dir.None;
    }

    private static bool HasDarkResDown()
    {
        var lp = Svc.Objects.LocalPlayer;
        if (lp == null) return false;
        return ((IBattleChara)lp).StatusList.Any(s => s.StatusId == DarkResistanceDown);
    }

    private static Vector2 ComputeBait(Dir n, bool darkRes)
    {
        if (C.Strat == Strat.DN)
        {
            return DnTable(n, RoleOf(C.Role), darkRes);
        }
        bool melee = IsMelee(C.Role);
        bool left = C.Strat == Strat.Relative
            ? (darkRes ? C.RelFireLeft : C.RelDarkLeft)
            : IsGroup1(C.Role);
        return darkRes ? StaticFire(n, melee, left) : StaticDark(n, melee, left);
    }

    private static float BaitRadius() => IsMelee(C.Role) ? 0.5f : 1.45f;
    private static string BaitText(bool darkDebuff) => darkDebuff ? "BAIT FIRE" : "BAIT DARK";
    private static Vector4 GuideColor(bool darkDebuff) => darkDebuff ? FireColor : DarkColor;

    private static bool IsMelee(Role r) => r is Role.MT or Role.OT or Role.M1 or Role.M2;

    private static bool IsGroup1(Role r) => r is Role.MT or Role.M1 or Role.H1 or Role.R1;

    private static RoleType RoleOf(Role r) => r switch
    {
        Role.MT or Role.OT => RoleType.Tank,
        Role.M1 or Role.M2 => RoleType.Melee,
        Role.H1 or Role.H2 => RoleType.Healer,
        _ => RoleType.Ranged,
    };

    private static Vector2 StaticFire(Dir n, bool melee, bool left) => (n, melee, left) switch
    {
        (Dir.NE, true, true) => new Vector2(101.365f, 98.635f),
        (Dir.NE, true, false) => new Vector2(101.365f, 98.635f),
        (Dir.NE, false, true) => new Vector2(86.350f, 100.000f),
        (Dir.NE, false, false) => new Vector2(100.000f, 113.650f),

        (Dir.SE, true, true) => new Vector2(101.365f, 101.365f),
        (Dir.SE, true, false) => new Vector2(101.365f, 101.365f),
        (Dir.SE, false, true) => new Vector2(100.000f, 86.350f),
        (Dir.SE, false, false) => new Vector2(86.350f, 100.000f),

        (Dir.SW, true, true) => new Vector2(98.635f, 101.365f),
        (Dir.SW, true, false) => new Vector2(98.635f, 101.365f),
        (Dir.SW, false, true) => new Vector2(113.650f, 100.000f),
        (Dir.SW, false, false) => new Vector2(100.000f, 86.350f),

        (Dir.NW, true, true) => new Vector2(98.635f, 98.635f),
        (Dir.NW, true, false) => new Vector2(98.635f, 98.635f),
        (Dir.NW, false, true) => new Vector2(100.000f, 113.650f),
        (Dir.NW, false, false) => new Vector2(113.650f, 100.000f),

        _ => Vector2.Zero,
    };

    private static Vector2 StaticDark(Dir n, bool melee, bool left) => (n, melee, left) switch
    {
        (Dir.NE, true, true) => new Vector2(93.175f, 98.635f),
        (Dir.NE, true, false) => new Vector2(101.365f, 106.825f),
        (Dir.NE, false, true) => new Vector2(100.000f, 86.350f),
        (Dir.NE, false, false) => new Vector2(113.650f, 100.000f),

        (Dir.SE, true, true) => new Vector2(101.365f, 93.175f),
        (Dir.SE, true, false) => new Vector2(93.175f, 101.365f),
        (Dir.SE, false, true) => new Vector2(113.650f, 100.000f),
        (Dir.SE, false, false) => new Vector2(100.000f, 113.650f),

        (Dir.SW, true, true) => new Vector2(106.825f, 101.365f),
        (Dir.SW, true, false) => new Vector2(98.635f, 93.175f),
        (Dir.SW, false, true) => new Vector2(100.000f, 113.650f),
        (Dir.SW, false, false) => new Vector2(86.350f, 100.000f),

        (Dir.NW, true, true) => new Vector2(98.635f, 106.825f),
        (Dir.NW, true, false) => new Vector2(106.825f, 98.635f),
        (Dir.NW, false, true) => new Vector2(86.350f, 100.000f),
        (Dir.NW, false, false) => new Vector2(100.000f, 86.350f),

        _ => Vector2.Zero,
    };

    private static Vector2 DnTable(Dir n, RoleType role, bool darkRes) => (n, role, darkRes) switch
    {
        (Dir.NE, RoleType.Tank, true) => new Vector2(101.365f, 98.635f),
        (Dir.NE, RoleType.Melee, true) => new Vector2(101.365f, 98.635f),
        (Dir.NE, RoleType.Healer, true) => new Vector2(100.000f, 113.650f),
        (Dir.NE, RoleType.Ranged, true) => new Vector2(100.000f, 113.650f),

        (Dir.SE, RoleType.Tank, true) => new Vector2(101.365f, 101.365f),
        (Dir.SE, RoleType.Melee, true) => new Vector2(101.365f, 101.365f),
        (Dir.SE, RoleType.Healer, true) => new Vector2(100.000f, 86.350f),
        (Dir.SE, RoleType.Ranged, true) => new Vector2(100.000f, 86.350f),

        (Dir.SW, RoleType.Tank, true) => new Vector2(98.635f, 101.365f),
        (Dir.SW, RoleType.Melee, true) => new Vector2(98.635f, 101.365f),
        (Dir.SW, RoleType.Healer, true) => new Vector2(100.000f, 86.350f),
        (Dir.SW, RoleType.Ranged, true) => new Vector2(100.000f, 86.350f),

        (Dir.NW, RoleType.Tank, true) => new Vector2(98.635f, 98.635f),
        (Dir.NW, RoleType.Melee, true) => new Vector2(98.635f, 98.635f),
        (Dir.NW, RoleType.Healer, true) => new Vector2(113.650f, 100.000f),
        (Dir.NW, RoleType.Ranged, true) => new Vector2(113.650f, 100.000f),

        (Dir.NE, RoleType.Tank, false) => new Vector2(101.365f, 106.825f),
        (Dir.NE, RoleType.Melee, false) => new Vector2(93.175f, 98.635f),
        (Dir.NE, RoleType.Healer, false) => new Vector2(100.000f, 86.350f),
        (Dir.NE, RoleType.Ranged, false) => new Vector2(113.650f, 100.000f),

        (Dir.SE, RoleType.Tank, false) => new Vector2(101.365f, 93.175f),
        (Dir.SE, RoleType.Melee, false) => new Vector2(93.175f, 101.365f),
        (Dir.SE, RoleType.Healer, false) => new Vector2(113.650f, 100.000f),
        (Dir.SE, RoleType.Ranged, false) => new Vector2(100.000f, 113.650f),

        (Dir.SW, RoleType.Tank, false) => new Vector2(98.635f, 93.175f),
        (Dir.SW, RoleType.Melee, false) => new Vector2(106.825f, 101.365f),
        (Dir.SW, RoleType.Healer, false) => new Vector2(100.000f, 113.650f),
        (Dir.SW, RoleType.Ranged, false) => new Vector2(86.350f, 100.000f),

        (Dir.NW, RoleType.Tank, false) => new Vector2(106.825f, 98.635f),
        (Dir.NW, RoleType.Melee, false) => new Vector2(98.635f, 106.825f),
        (Dir.NW, RoleType.Healer, false) => new Vector2(100.000f, 86.350f),
        (Dir.NW, RoleType.Ranged, false) => new Vector2(86.350f, 100.000f),

        _ => Vector2.Zero,
    };

    private static void Resolve(ulong master, List<ulong> clones, Vector4 color, Dictionary<ulong, Vector4> desired)
    {
        if (master == 0)
        {
            return;
        }
        IGameObject masterObj = master.GameObject();
        if (clones.Count < 2 && masterObj != null)
        {
            foreach (IGameObject obj in YapYapDraw.Plugin.ObjectTable)
            {
                if (obj.BaseId != CloneBaseId || clones.Contains(obj.GameObjectId))
                {
                    continue;
                }
                if (Math.Abs(Vector3.Distance(masterObj.Position, obj.Position) - 5f) <= 0.5f)
                {
                    clones.Add(obj.GameObjectId);
                }
            }
            if (clones.Count < 2)
            {
                clones.Clear();
            }
        }
        if (clones.Count >= 2)
        {
            foreach (ulong id in clones)
            {
                desired[id] = color;
            }
        }
        else if (masterObj != null)
        {
            desired[master] = color;
        }
    }

    private static StaticVfx Spawn(IGameObject clone, Vector4 color)
    {
        return DrawManager.Draw(new DrawElement
        {
            drawAvfx = "customCircle",
            radiusX = 3f,
            radiusZ = 3f,
            drawOnObject = true,
            refColor = color,
            refTargetColor = color,
            destroyTime = 600000f
        }, clone);
    }

    private static StaticVfx SpawnBait(Vector3 pos, float radius)
    {
        return DrawManager.Draw(new DrawElement
        {
            drawAvfx = "customCircle",
            radiusX = radius,
            radiusZ = radius,
            drawOnObject = false,
            Position = pos,
            refColor = BaitColor,
            refTargetColor = BaitColor,
            destroyTime = 600000f
        });
    }

    private void RefreshGuide(bool darkDebuff, Vector3 spot)
    {
        if (Plugin.Instance == null) return;

        bool spotSame = _lastGuideSpot.HasValue && Vector3.Distance(_lastGuideSpot.Value, spot) < 0.05f;
        if (_guideLive && _lastGuideDark == darkDebuff && spotSame && _lastTether == C.ShowTether)
            return;

        _guideLive     = true;
        _lastGuideDark = darkDebuff;
        _lastGuideSpot = spot;
        _lastTether    = C.ShowTether;

        var color = GuideColor(darkDebuff);
        var e = new LogEvent { Name = "rep1_guide" };

        Plugin.Instance.Engine.ClearExternal(GuideOwner);

        // Text rides on the player, not the spot.
        Plugin.Instance.Engine.SpawnExternal(GuideOwner, new DrawSpec
        {
            Shape = QuickShape.Text,
            Anchor = DrawAnchor.Self,
            AttachToActor = true,
            Color = color,
            Duration = 600f,
            Label = BaitText(darkDebuff),
            LabelColor = color,
            LabelSize = 1.2f,
            LabelHeight = 2f,
        }, e, previewSelf: true);

        if (!C.ShowTether) return;

        Plugin.Instance.Engine.SpawnExternal(GuideOwner, new DrawSpec
        {
            Shape = QuickShape.ChevronPath,
            Anchor = DrawAnchor.Self,
            AttachToActor = true,
            Link = LinkTarget.FixedSpot,
            LinkPosition = spot,
            Color = color,
            ChevronSpacing = 2f,
            LineThickness = 4f,
            Length = 30f,
            Duration = 600f,
        }, e, previewSelf: true);
    }

    private void RemoveBait()
    {
        if (_bait != null)
        {
            _bait.Remove();
            aoes.Remove(_bait);
            _bait = null;
        }
        _baitAt = null;
    }

    private void ClearGuide()
    {
        if (!_guideLive && _lastGuideSpot == null) return;
        _guideLive = false;
        _lastGuideDark = null;
        _lastGuideSpot = null;
        Plugin.Instance?.Engine.ClearExternal(GuideOwner);
    }

    private void ClearAll()
    {
        foreach (StaticVfx vfx in _rings.Values)
        {
            vfx?.Remove();
        }
        _rings.Clear();
        RemoveBait();
        ClearGuide();
        aoes.Clear();
        _darkMaster = 0;
        _fireMaster = 0;
        _phase = 0;
        _north = Dir.None;
        _darkClones.Clear();
        _fireClones.Clear();
    }
}
