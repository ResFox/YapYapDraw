using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using YapYapDraw.Engine.Interop;
using YapYapDraw.Engine.Interop.Game;
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

// M12S P2 Idyllic Dream: 17-phase state machine for uptime defamations and stacks.
public unsafe class IdyllicDream : ISpecialAction
{
    public enum Dir { N, NE, E, SE, S, SW, W, NW }

    public enum Towers : uint { WindLight = 2015013, DoomLight = 2015014, Fire = 2015016, Earth = 2015015 }

    public enum TetherKind : uint { Stack = 369, Defamation = 368 }

    public enum TowerPosition { MeleeLeft, MeleeRight, RangedLeft, RangedRight }

    public enum PickupOrder { Defamation_1, Defamation_2, Defamation_3, Defamation_4, Stack_1, Stack_2, Stack_3, Stack_4 }

    private const int CurrentConfigVersion = 2;

    // Defaults below mirror the "EU Hector Uptime Caro (Defams NW/NE) Group 1" preset.
    public class Config
    {
        public int ConfigVersion = CurrentConfigVersion;
        public bool Active;

        public TowerPosition TowerPosition = TowerPosition.RangedRight;
        public bool IsGroup1 = true;               // West platform
        public bool TakenCheckConditionIsTakenTower = true;
        public bool TakenFarIsEarth;               // wind/far baited by the Fire-tower player
        public bool TakenFarIsMelee = true;
        public bool DontShowElementsP11S1;

        public List<PickupOrder> Pickups = new()
        {
            PickupOrder.Defamation_1, PickupOrder.Stack_1, PickupOrder.Stack_2, PickupOrder.Defamation_2,
            PickupOrder.Defamation_3, PickupOrder.Stack_3, PickupOrder.Stack_4, PickupOrder.Defamation_4,
        };

        public bool AltCloneResolution = true;
        public List<Dir> AltCloneDirections = new() { Dir.W, Dir.SW };
        public bool StackEnumPrioHorizontal;
        public bool StackEnumVerticalNorth = true;
        public bool StackEnumHorizontalWest = true;

        public HashSet<Dir> LP2CardinalStackFirst = new() { Dir.N, Dir.NE, Dir.E, Dir.SE };
        public HashSet<Dir> LP2CardinalDefamationFirst = new() { Dir.N, Dir.NE, Dir.E, Dir.SE };

        public bool ShowTetherLine = true;
        public bool ShowTetherCircle = true;
        public bool ShowGuidePath = true;
        public bool ShowGuideText = true;
        public bool SkipIndiMechs;
        public int ColorIndex = 4;

        public bool Preview;
    }

    private sealed class TowerData
    {
        public Dir Side;
        public Towers Kind;
        public Vector3 Position = Vector3.Zero;
        public uint AssignToPlayerEntityId;
    }

    private static bool _enableMigrated;
    private static bool _configMigrated;

    private static Config C => ModuleConfig.Get<Config>();

    private static void EnsureEnableMigrated()
    {
        if (!_configMigrated)
        {
            _configMigrated = true;
            if (C.ConfigVersion < CurrentConfigVersion)
                ModuleConfig.Set(new Config());
        }
        if (_enableMigrated) return;
        _enableMigrated = true;
        ModuleConfig.MigrateLegacyActive("Lindblum/Idyllic Dream (Uptime)", C.Active);
    }

    private const uint PlayerCloneBaseId = 19210u;
    private const uint BossCloneNameId = 14380u;
    private const float Alpha = 0.85f;

    private static readonly Dictionary<Dir, Vector2> ReenactmentDirections = new()
    {
        [Dir.N] = new(100, 86), [Dir.NE] = new(110, 90), [Dir.E] = new(114, 100), [Dir.SE] = new(110, 110),
        [Dir.S] = new(100, 114), [Dir.SW] = new(90, 110), [Dir.W] = new(86, 100), [Dir.NW] = new(90, 90),
    };

    private static readonly int[] ReenactSeqCardinalA = { 0, 2, 4, 6 };
    private static readonly int[] ReenactSeqIntercardA = { 1, 3, 5, 7 };
    private static readonly int[] ReenactSeqCardinalB = { 1, 3, 5, 7 };
    private static readonly int[] ReenactSeqIntercardB = { 0, 2, 4, 6 };

    private int _phase;
    private int _phase7Sub;
    private int _phase11Sub;
    private int _defamationAttack;
    private int _playerPosition = -1;
    private bool? _isCardinalFirst;
    private bool? _isThDecreasingResistance;
    private bool? _isConeSafeNorth;
    private bool? _nextCleavesNorthSouth;
    private Vector3? _nextAOE;
    private readonly HashSet<(Vector3 Pos, float Rot)> _nextCleaves = new();
    private readonly Dictionary<uint, Vector3> _clonePositions = new(); // player entityId -> clone pos
    private readonly Dictionary<uint, bool> _defamationPlayers = new(); // player entityId -> isDefamation
    private readonly Dictionary<uint, int> _playerOrder = new();        // player entityId -> clockwise idx
    private readonly Dictionary<uint, (uint tetherId, uint playerId)> _cloneTethers = new();
    private long _captureTowersAt;
    private bool _towersCaptured;
    private TowerData[] _towers = MakeTowerArray();

    private static TowerData[] MakeTowerArray() => new TowerData[]
    {
        new() { Side = Dir.W, Kind = Towers.Fire }, new() { Side = Dir.W, Kind = Towers.Earth },
        new() { Side = Dir.W, Kind = Towers.WindLight }, new() { Side = Dir.W, Kind = Towers.DoomLight },
        new() { Side = Dir.E, Kind = Towers.Fire }, new() { Side = Dir.E, Kind = Towers.Earth },
        new() { Side = Dir.E, Kind = Towers.WindLight }, new() { Side = Dir.E, Kind = Towers.DoomLight },
    };

    private readonly Dictionary<string, StaticVfx> _el = new();
    private bool _built;

    private const string GuideOwner = "m12s_idyllic_guide";
    private Vector3? _stackFinal;
    private bool _guideLive;
    private Vector3? _lastGuideSpot;
    private string _lastGuideLabel = "";
    private bool _lastShowText;
    private bool _lastShowPath;

    public static bool IsRunning { get; private set; }

    public override string Name => "Idyllic Dream (Uptime)";
    public override string? ModuleEnableKey => "Lindblum/Idyllic Dream (Uptime)";

    public override bool Registered => false;

    public override uint Phase => 2u;

    public override bool HasConfig => true;

    public override HashSet<uint> ActionID => new()
    {
        46345u, 48098u, 46358u, 46360u, 46361u, 48099u, 46356u, 46367u, 46327u, 46330u, 46324u, 46352u,
    };

    private static readonly Vector4 DefaColor = new(0.45f, 0.40f, 1f, 0.55f);
    private static readonly Vector4 StackColor = new(0.10f, 0.75f, 0.40f, 0.50f);
    private static readonly Vector4 ConeColor = new(1f, 0.80f, 0.10f, 0.30f);
    private static readonly Vector4 MeteorColor = new(1f, 0.55f, 0.10f, 0.32f);
    private static readonly Vector4 SafeColor = new(0.20f, 0.90f, 1f, 0.50f);
    private static readonly Vector4 TowerColor = new(0.20f, 0.95f, 0.35f, 0.60f);

    private static Vector4 Guide
    {
        get
        {
            float t = (Environment.TickCount64 % 2000L) / 2000f;
            Vector4 c = HsvToRgb(t, 1f, 1f);
            c.W = Alpha;
            return c;
        }
    }

    public override void OnActionCast(ActorCastInfo info)
    {
        if (info.ActionId == 46345 && _phase == 0) _phase = 1;
        if (info.ActionId == 48098) _phase++;
        if (info.ActionId == 46352 && (_phase == 3 || _phase == 4))
            _isConeSafeNorth = info.Pos.Z < 100f;
    }

    public override void OnAbilityCast(ActorAbilityInfo info)
    {
        switch (info.ActionId)
        {
            case 46345: _phase = 1; break;
            case 48098: _phase++; break;
            case 46358:
                _nextAOE = null;
                _nextCleaves.Clear();
                if (_phase == 17)
                {
                    _nextCleavesNorthSouth = null;
                    Reset();
                }
                break;
            case 46360:
            case 46361:
                if (_phase == 9) _defamationAttack++;
                break;
            case 48099:
                if (_phase is 13 or 14 or 16 or 17) _defamationAttack++;
                break;
            case 46356:
                if (_phase == 7 && _phase7Sub == 0) _phase7Sub++;
                break;
            case 46367:
                if (_phase == 7) _captureTowersAt = Environment.TickCount64 + 1000;
                break;
            case 46327:
                if (_phase is 10 or 11 && _phase11Sub == 1) _phase11Sub++;
                break;
            case 46330:
                if (_phase is 10 or 11 && _phase11Sub == 2) _phase11Sub++;
                break;
            case 46324:
                if (_phase is 10 or 11) AssignTower(info);
                break;
        }
    }

    public override void OnAddStatus(ActorStatusChangeInfo info)
    {
        if (_phase == 7 && info.StatusID == 4164 && !_isThDecreasingResistance.HasValue)
        {
            IGameObject? pc = ((uint)info.TargetID).GameObject();
            if (pc is IPlayerCharacter ipc)
                _isThDecreasingResistance = ipc.GetRole() != CombatRole.DPS;
        }
        if (_phase is 10 or 11 && _phase11Sub == 0 && info.StatusID is 4766u or 4767u)
            _phase11Sub++;
    }

    public override void OnActorPlayActionTimelineEvent(IGameObject source, uint id)
    {
        if (_phase == 1 && _isCardinalFirst == null && id == 4562u && source != null)
        {
            Vector2 pos = V2(source.Position);
            if (Vector2.Distance(pos, new Vector2(100f, 86f)) < 2f) _isCardinalFirst = true;
            else if (Vector2.Distance(pos, new Vector2(110f, 90f)) < 2f) _isCardinalFirst = false;
        }
    }

    public override void OnActorTetherEvent(uint actorId, uint id, ulong targetId)
    {
        if (id is 367u or 368u or 369u or 373u or 374u)
            _cloneTethers[actorId] = (id, (uint)targetId);
    }

    public override void OnActorTetherCancelEvent(uint actorId)
    {
        _cloneTethers.Remove(actorId);
    }

    public override void Update()
    {
        EnsureEnableMigrated();
        if (C.Preview) { IsRunning = false; Build(); HideAll(); ClearGuide(); DrawPreview(); return; }
        if (!ModuleConfig.IsEnabled(ModuleEnableKey)) { IsRunning = false; HideAll(); ClearGuide(); return; }
        if (_phase <= 0) { IsRunning = false; HideAll(); ClearGuide(); return; }
        IsRunning = true;
        Build();
        HideAll();
        _stackFinal = null;

        IPlayerCharacter? me = Svc.Objects.LocalPlayer;
        if (me == null) { ClearGuide(); return; }

        if (_captureTowersAt != 0 && Environment.TickCount64 >= _captureTowersAt)
            CaptureTowers();

        switch (_phase)
        {
            case 1: Phase1(); break;
            case 2: ScanCleaves(46354u, 1f, 2f, 46353u); break;
            case 4: ScanCleaves(46354u, 1f, 999f, 46353u); break;
        }

        if (_phase is 5 or 6) Phase5(me);
        if (_phase == 7 && _phase7Sub == 0) Phase7Cone();
        if (_phase == 7 && _phase7Sub == 1) Phase7Tower(me);
        if (_phase == 9 && Adj() < 4) Phase9(me);
        if (_phase is 10 or 11) Phase1011(me);
        if (_phase == 12) ScanCleaves12();
        if (_phase is 13 or 14) ReenactA();
        if (_phase is 16 or 17) ReenactB();
        if ((_phase == 13 && Adj() < 5) || ((_phase == 16 || _phase == 17) && Adj() < 6)) StackTether();
        if (_phase is 12 or 13 or 14 or 15) SafePlatform();

        if (_phase is 6 or 7 or 8) ShowStoredCleaves(_phase == 6 ? 0.2f : 0.5f);
        if (_phase is 14 or 15 or 16) ShowStoredCleaves(_phase == 14 ? 0.2f : 0.5f);
        if (_phase == 17) PortalCones();

        UpdatePersonalGuide(me);
    }

    // phase 1: capture player-clone tethers + positions
    private void Phase1()
    {
        var clones = EnumCw(
            Svc.Objects.Where(o => o.BaseId == PlayerCloneBaseId && _cloneTethers.ContainsKey(o.EntityId)),
            new Vector2(100, 100), new Vector2(96, 86));
        if (clones.Count != 8) return;

        uint meId = Svc.Objects.LocalPlayer?.EntityId ?? 0;
        for (int i = 0; i < clones.Count; i++)
        {
            IGameObject c = clones[i];
            if (_cloneTethers.TryGetValue(c.EntityId, out var t))
            {
                if (t.playerId == meId) _playerPosition = i;
                _clonePositions[t.playerId] = c.Position;
            }
        }
        _nextCleaves.Clear();
    }

    // phase 5/6: classify boss clones, point me to my tether
    private void Phase5(IPlayerCharacter me)
    {
        var clones = GetBossClones();
        if (clones.Count != 8) return;

        PickupOrder mine = C.Pickups[Math.Clamp(_playerPosition, 0, 7)];
        bool meDefa = (int)mine <= 3;
        int myOrder = meDefa ? (int)mine : (int)mine - 4;
        int defaClone = 0, stackClone = 0;

        for (int i = 0; i < clones.Count; i++)
        {
            IGameObject c = clones[i];
            if (!_cloneTethers.TryGetValue(c.EntityId, out var t)) continue;
            IGameObject? tgt = t.playerId.GameObject();
            Vector3 tgtPos = tgt?.Position ?? me.Position;
            _playerOrder[t.playerId] = i;

            if (t.tetherId == (uint)TetherKind.Defamation)
            {
                _defamationPlayers[t.playerId] = true;
                if (meDefa && defaClone == myOrder)
                    PointPick(c, tgtPos);
                defaClone++;
            }
            else if (t.tetherId == (uint)TetherKind.Stack)
            {
                _defamationPlayers[t.playerId] = false;
                if (!meDefa && stackClone == myOrder)
                    PointPick(c, tgtPos);
                stackClone++;
            }
        }
    }

    private void PointPick(IGameObject clone, Vector3 playerPos)
    {
        if (C.SkipIndiMechs) return;
        if (C.ShowTetherLine) ShowLine("PickTether", clone.Position, playerPos, Guide);
        if (C.ShowTetherCircle) ShowAt("PickTetherCircle", clone.Position, Guide);
    }

    // stored cleaves/meteor telegraph (phases 6/7/8 and 14/15/16)
    private void ShowStoredCleaves(float intensity)
    {
        Vector4 cone = ConeColor with { W = ConeColor.W * (intensity / 0.5f) };
        Vector4 meteor = MeteorColor with { W = MeteorColor.W * (intensity / 0.5f) };
        int i = 0;
        foreach (var (pos, rot) in _nextCleaves)
        {
            i++;
            ShowCone($"Cone{i}", pos, rot, cone);
        }
        if (_nextAOE != null) ShowAt("Circle", _nextAOE.Value, meteor);
    }

    // config preview: show the fixed "stand here" positions out of combat
    private void DrawPreview()
    {
        ShowAt("DefamationGroup1", DefaGroupPos(1), DefaColor);
        ShowAt("DefamationGroup2", DefaGroupPos(2), DefaColor);
        ShowAt("StackGroup1", StackGroupPos(1), StackColor);
        ShowAt("StackGroup2", StackGroupPos(2), StackColor);
        ShowAt("SafespotGroup1", SafespotPos(1), Guide);
        ShowAt("SafespotGroup2", SafespotPos(2), Guide);
        ShowAt("Given Far", BaitPos("Given Far"), TowerColor);
        ShowAt("Given Near", BaitPos("Given Near"), TowerColor);
        ShowAt("Taken Far", BaitPos("Taken Far"), TowerColor);
        ShowAt("Taken Near", BaitPos("Taken Near"), TowerColor);
    }

    // phase 7 sub 0: avoid cone, stand on safe cardinal half
    private void Phase7Cone()
    {
        if (!_isConeSafeNorth.HasValue) return;
        float z = _isConeSafeNorth.Value ? 90f : 110f;
        float x = C.IsGroup1 ? 90f : 110f;
        ShowAt("p7sub1 tether", new Vector3(x, 0f, z), Guide);
    }

    // phase 7 sub 1: tower tether + light spread
    private void Phase7Tower(IPlayerCharacter me)
    {
        Vector3 baseMelee = new(90.243f, 0f, 95.757f);
        Vector3 baseRanged = new(81.757f, 0f, 95.757f);
        Vector3 pos = C.TowerPosition switch
        {
            TowerPosition.MeleeLeft => baseMelee,
            TowerPosition.MeleeRight => baseMelee with { Z = 200f - baseMelee.Z },
            TowerPosition.RangedLeft => baseRanged,
            TowerPosition.RangedRight => baseRanged with { Z = 200f - baseRanged.Z },
            _ => baseMelee,
        };
        if (!C.IsGroup1) pos = pos with { X = 200f - pos.X, Z = 200f - pos.Z };
        ShowAt("TowerTether", pos, Guide);
        ShowAt("P7AOERadius", me.Position, MeteorColor);
    }

    // phase 9: defamations + stacks (light party split)
    private void Phase9(IPlayerCharacter me)
    {
        int n = Adj();
        uint g2 = FindByOrder(0 + n);
        uint g1 = FindByOrder(4 + n);
        if (g2 == 0 || g1 == 0) return;
        IGameObject? og2 = g2.GameObject();
        IGameObject? og1 = g1.GameObject();
        if (og2 == null || og1 == null) return;
        if (!_defamationPlayers.TryGetValue(g2, out bool g2Defa)) return;
        if (!_defamationPlayers.TryGetValue(g1, out bool g1Defa)) return;

        Dir myDir = _playerOrder.TryGetValue(me.EntityId, out int mo) ? (Dir)mo : Dir.N;
        uint firstId = FindByOrder(0);
        bool firstDefa = firstId != 0 && _defamationPlayers.TryGetValue(firstId, out bool fd) && fd;
        var set = firstDefa ? C.LP2CardinalDefamationFirst : C.LP2CardinalStackFirst;
        int party = set.Contains(myDir) ? 2 : 1;

        bool defaOnYou = (g2 == me.EntityId && g2Defa) || (g1 == me.EntityId && g1Defa);
        if (defaOnYou) ShowAt("DefamationOnYou", me.Position, DefaColor);

        if (g2Defa && !defaOnYou && !C.SkipIndiMechs)
            ShowAt($"SafespotGroup{party}", SafespotPos(party), Guide);

        if (g2Defa)
        {
            ShowAt("Defamation2", og2.Position, DefaColor);
            if (defaOnYou && party == 2 && !C.SkipIndiMechs)
                ShowAt("DefamationGroup2", DefaGroupPos(2), Guide);
        }
        if (g1Defa)
        {
            ShowAt("Defamation1", og1.Position, DefaColor);
            if (defaOnYou && party == 1 && !C.SkipIndiMechs)
                ShowAt("DefamationGroup1", DefaGroupPos(1), Guide);
        }
        if (!g2Defa)
        {
            ShowAt("Stack2", og2.Position, StackColor);
            if (party == 2 && !C.SkipIndiMechs)
                ShowAt("StackGroup2", StackGroupPos(2), Guide);
        }
        if (!g1Defa)
        {
            ShowAt("Stack1", og1.Position, StackColor);
            if (party == 1 && !C.SkipIndiMechs)
                ShowAt("StackGroup1", StackGroupPos(1), Guide);
        }
    }

    // phase 10/11: towers, light effects, near/far baits
    private void Phase1011(IPlayerCharacter me)
    {
        if (_phase11Sub == 0)
        {
            IGameObject? tower = GetShouldTakeTower(me);
            if (tower != null)
            {
                uint nameId = TowerKind(tower);
                bool isMelee = C.TowerPosition is TowerPosition.MeleeRight or TowerPosition.MeleeLeft;
                Vector3 p = tower.Position;
                if (nameId == (uint)Towers.DoomLight)
                {
                    if (isMelee) p += new Vector3(0, 0, tower.Position.Z > 100 ? 1.5f : -1.5f);
                    else p += new Vector3(tower.Position.X > 100 ? 1.5f : -1.5f, 0, 0);
                }
                else if (nameId == (uint)Towers.WindLight)
                {
                    p += new Vector3(tower.Position.X > 100 ? -1.5f : 1.5f, 0, 0);
                }
                ShowAt("TowerTether", p, Guide);
            }
        }
        else if (_phase11Sub == 1)
        {
            if (me.StatusList.Any(s => s.StatusId == 4768))
                ShowAt("TowerTether", me.Position, Guide);
            var earth = _towers.Where(t => t.Kind == Towers.Earth).ToList();
            for (int i = 0; i < earth.Count; i++)
                ShowAt($"Rock{i + 1}", earth[i].Position, TowerColor);
        }
        else if (_phase11Sub == 2 && !C.DontShowElementsP11S1)
        {
            Phase11Cones(me);
        }
    }

    private void Phase11Cones(IPlayerCharacter me)
    {
        var pcs = Svc.Objects.OfType<IPlayerCharacter>().ToList();
        var far = pcs.Where(x => x.StatusList.Any(s => s.StatusId == 4766)).ToList();
        var near = pcs.Where(x => x.StatusList.Any(s => s.StatusId == 4767)).ToList();
        if (far.Count + near.Count == 4)
        {
            for (int i = 0; i < far.Count; i++)
            {
                IPlayerCharacter b = far[i];
                IPlayerCharacter? farthest = pcs.OrderByDescending(x => Vector3.DistanceSquared(x.Position, b.Position)).FirstOrDefault();
                if (farthest != null) ShowConeBetween($"FarCone{i + 1}", b.Position, farthest.Position);
            }
            for (int i = 0; i < near.Count; i++)
            {
                IPlayerCharacter b = near[i];
                IPlayerCharacter? nearest = pcs.OrderBy(x => Vector3.Distance(x.Position, b.Position)).Skip(1).FirstOrDefault();
                if (nearest != null) ShowConeBetween($"NearCone{i + 1}", b.Position, nearest.Position);
            }
        }

        if (!C.TakenCheckConditionIsTakenTower)
        {
            bool isMelee = C.TowerPosition is TowerPosition.MeleeRight or TowerPosition.MeleeLeft;
            string name = (C.TakenFarIsMelee, isMelee) switch
            {
                (true, true) => "Taken Far",
                (true, false) => "Taken Near",
                (false, true) => "Taken Near",
                (false, false) => "Taken Far",
            };
            if (me.StatusList.Any(s => s.StatusId == 4766)) name = "Given Far";
            else if (me.StatusList.Any(s => s.StatusId == 4767)) name = "Given Near";
            ShowBait(name, me);
        }
        else
        {
            uint earthId = C.IsGroup1
                ? _towers.FirstOrDefault(x => x.Kind == Towers.Earth && x.Side == Dir.W)?.AssignToPlayerEntityId ?? 0
                : _towers.FirstOrDefault(x => x.Kind == Towers.Earth && x.Side == Dir.E)?.AssignToPlayerEntityId ?? 0;
            uint fireId = C.IsGroup1
                ? _towers.FirstOrDefault(x => x.Kind == Towers.Fire && x.Side == Dir.W)?.AssignToPlayerEntityId ?? 0
                : _towers.FirstOrDefault(x => x.Kind == Towers.Fire && x.Side == Dir.E)?.AssignToPlayerEntityId ?? 0;
            if (earthId == 0 || fireId == 0) return;
            bool isEarth = me.EntityId == earthId;
            string name = (isEarth && C.TakenFarIsEarth) || (!isEarth && !C.TakenFarIsEarth) ? "Taken Far" : "Taken Near";
            if (isEarth && me.StatusList.Any(s => s.StatusId == 4767)) name = "Given Near";
            else if (!isEarth && me.StatusList.Any(s => s.StatusId == 4766)) name = "Given Far";
            if (me.StatusList.Any(s => s.StatusId == 4766)) name = "Given Far";
            else if (me.StatusList.Any(s => s.StatusId == 4767)) name = "Given Near";
            ShowBait(name, me);
        }
    }

    private void ShowBait(string name, IPlayerCharacter me)
    {
        Vector3 p = BaitPos(name);
        if ((me.Position.X > 100 && p.X < 100) || (me.Position.X < 100 && p.X > 100))
            p = p with { X = 200f - p.X };
        ShowLine(name, me.Position, p, Guide);
    }

    private IGameObject? GetShouldTakeTower(IPlayerCharacter me)
    {
        var nonLight = Svc.Objects.Where(x => TowerKind(x) is (uint)Towers.Fire or (uint)Towers.Earth);
        var assignedNonLight = C.IsGroup1 ? nonLight.Where(x => x.Position.X < 100) : nonLight.Where(x => x.Position.X > 100);
        var light = Svc.Objects.Where(x => TowerKind(x) is (uint)Towers.WindLight or (uint)Towers.DoomLight);
        var assignedLight = C.IsGroup1 ? light.Where(x => x.Position.X < 100) : light.Where(x => x.Position.X > 100);
        if (assignedNonLight.Count() + assignedLight.Count() != 4) return null;
        if (!_isThDecreasingResistance.HasValue) return null;

        bool isDps = me.GetRole() == CombatRole.DPS;
        bool canLight = isDps == _isThDecreasingResistance.Value;
        bool isMelee = C.TowerPosition is TowerPosition.MeleeRight or TowerPosition.MeleeLeft;
        Vector2 center = new(100, 100);
        return (isMelee, canLight) switch
        {
            (true, false) => assignedNonLight.OrderBy(x => Vector2.Distance(V2(x.Position), center)).FirstOrDefault(),
            (true, true) => assignedLight.OrderBy(x => Vector2.Distance(V2(x.Position), center)).FirstOrDefault(),
            (false, false) => assignedNonLight.OrderByDescending(x => Vector2.Distance(V2(x.Position), center)).FirstOrDefault(),
            (false, true) => assignedLight.OrderByDescending(x => Vector2.Distance(V2(x.Position), center)).FirstOrDefault(),
        };
    }

    private void AssignTower(ActorAbilityInfo info)
    {
        Vector3 src = info.Source?.Position ?? info.Pos;
        TowerData? tower = _towers.FirstOrDefault(x => Vector2.Distance(V2(x.Position), V2(src)) < 2f);
        if (tower == null) return;
        IPlayerCharacter? pc = Svc.Objects.OfType<IPlayerCharacter>().FirstOrDefault(x => Vector2.Distance(V2(x.Position), V2(src)) < 2f);
        if (pc != null) tower.AssignToPlayerEntityId = pc.EntityId;
    }

    private void CaptureTowers()
    {
        _captureTowersAt = 0;
        foreach (IGameObject t in Svc.Objects)
        {
            uint id = TowerKind(t);
            if (id is not ((uint)Towers.Fire or (uint)Towers.Earth or (uint)Towers.WindLight or (uint)Towers.DoomLight))
                continue;
            Dir ew = t.Position.X > 100 ? Dir.E : Dir.W;
            TowerData? slot = _towers.FirstOrDefault(x => x.Side == ew && (uint)x.Kind == id);
            if (slot != null) slot.Position = t.Position;
        }
        _towersCaptured = true;
    }

    // phase 12 cleave scan (N/S vs E/W + meteor)
    private void ScanCleaves12()
    {
        _nextCleaves.Clear();
        foreach (IGameObject o in Svc.Objects)
        {
            if (o is not IBattleChara bc || !bc.IsCasting) continue;
            float ct = bc.CurrentCastTime;
            if (bc.CastActionId == 46352 && ct >= 1f && ct <= 2f)
            {
                _nextCleaves.Add((bc.Position, 0f));
                _nextCleaves.Add((bc.Position, MathF.PI));
                _nextCleavesNorthSouth = false;
            }
            if (bc.CastActionId == 46351 && ct >= 1f && ct <= 2f)
            {
                _nextCleaves.Add((bc.Position, MathF.PI / 2f));
                _nextCleaves.Add((bc.Position, 3f * MathF.PI / 2f));
                _nextCleavesNorthSouth = true;
            }
            if (bc.CastActionId == 48303 && ct >= 1f && ct <= 2f)
                _nextAOE = bc.Position;
        }
    }

    private void ReenactA()
    {
        if (Adj() >= 5) return;
        if (_isCardinalFirst == true) { Stored(0); Stored(2); Stored(4); Stored(6); }
        else if (_isCardinalFirst == false) { Stored(1); Stored(3); Stored(5); Stored(7); }
    }

    private void ReenactB()
    {
        if (Adj() >= 6) return;
        if (_isCardinalFirst == true) { Stored(1); Stored(3); Stored(5); Stored(7); }
        else if (_isCardinalFirst == false) { Stored(0); Stored(2); Stored(4); Stored(6); }
    }

    private void Stored(int index)
    {
        var ordered = EnumCwKv(_clonePositions, new Vector2(100, 100), new Vector2(98, 86));
        if (index >= ordered.Count) return;
        uint pid = ordered[index].Key;
        if (!_defamationPlayers.TryGetValue(pid, out bool isDefa)) return;
        string[] keys = isDefa ? new[] { "Defamation1", "Defamation2" } : new[] { "Stack1", "Stack2" };
        foreach (string k in keys)
        {
            if (_el.TryGetValue(k, out StaticVfx? e) && !e.Enable)
            {
                ShowAt(k, ordered[index].Value, isDefa ? DefaColor : StackColor);
                return;
            }
        }
    }

    private void StackTether()
    {
        Vector3 s1 = ElPos("Stack1");
        Vector3 s2 = ElPos("Stack2");
        var stackPos = new List<Vector3> { s1, s2 };
        Vector3? final = null;

        if (C.AltCloneResolution)
        {
            Vector3 pick = stackPos.FirstOrDefault(x =>
                C.AltCloneDirections.Any(a => Vector2.Distance(V2(x), ReenactmentDirections[a]) < 2f));
            if (pick != default) final = pick;
        }
        else if (C.StackEnumPrioHorizontal)
        {
            if (Approx(stackPos[0].X, stackPos[1].X, 1f))
            {
                stackPos = stackPos.OrderBy(x => x.Z).ToList();
                final = stackPos[C.StackEnumVerticalNorth ? 0 : 1];
            }
            else
            {
                stackPos = stackPos.OrderBy(x => x.X).ToList();
                final = stackPos[C.StackEnumHorizontalWest ? 0 : 1];
            }
        }
        else
        {
            if (Approx(stackPos[0].Z, stackPos[1].Z, 1f))
            {
                stackPos = stackPos.OrderBy(x => x.X).ToList();
                final = stackPos[C.StackEnumHorizontalWest ? 0 : 1];
            }
            else
            {
                stackPos = stackPos.OrderBy(x => x.Z).ToList();
                final = stackPos[C.StackEnumVerticalNorth ? 0 : 1];
            }
        }

        _stackFinal = final;
        if (final != null && !C.SkipIndiMechs)
            ShowAt("stack tether", final.Value, Guide);
    }

    private int MyParty(IPlayerCharacter me)
    {
        Dir myDir = _playerOrder.TryGetValue(me.EntityId, out int mo) ? (Dir)mo : Dir.N;
        uint firstId = FindByOrder(0);
        bool firstDefa = firstId != 0 && _defamationPlayers.TryGetValue(firstId, out bool fd) && fd;
        HashSet<Dir> setDirs = firstDefa ? C.LP2CardinalDefamationFirst : C.LP2CardinalStackFirst;
        return setDirs.Contains(myDir) ? 2 : 1;
    }

    private int ReenactActiveOrderIndex()
    {
        if (!_isCardinalFirst.HasValue) return -1;
        int adj = Adj();
        int[] seq = _phase is 13 or 14
            ? (_isCardinalFirst == true ? ReenactSeqCardinalA : ReenactSeqIntercardA)
            : (_phase is 16 or 17
                ? (_isCardinalFirst == true ? ReenactSeqCardinalB : ReenactSeqIntercardB)
                : Array.Empty<int>());
        if (_phase is 13 or 14 && adj >= seq.Length) return -1;
        if (_phase is 16 or 17 && adj >= seq.Length) return -1;
        return adj < seq.Length ? seq[adj] : -1;
    }

    private Vector3 DefaSpotFor(IPlayerCharacter me)
    {
        if (_el.TryGetValue("Defamation1", out StaticVfx? e1) && e1.Enable && e1.Position != Vector3.Zero)
            return e1.Position;
        if (_el.TryGetValue("Defamation2", out StaticVfx? e2) && e2.Enable && e2.Position != Vector3.Zero)
            return e2.Position;
        if (_clonePositions.TryGetValue(me.EntityId, out Vector3 clonePos))
            return DefaGroupPos(clonePos.X < 100f ? 1 : 2);
        return DefaGroupPos(MyParty(me));
    }

    private Vector3? TowerGuideSpot(IPlayerCharacter me)
    {
        if (_phase == 7 && _phase7Sub == 0 && _isConeSafeNorth.HasValue)
        {
            float z = _isConeSafeNorth.Value ? 90f : 110f;
            float x = C.IsGroup1 ? 90f : 110f;
            return new Vector3(x, 0f, z);
        }
        if (_phase == 7 && _phase7Sub == 1)
        {
            Vector3 baseMelee = new(90.243f, 0f, 95.757f);
            Vector3 baseRanged = new(81.757f, 0f, 95.757f);
            Vector3 pos = C.TowerPosition switch
            {
                TowerPosition.MeleeLeft => baseMelee,
                TowerPosition.MeleeRight => baseMelee with { Z = 200f - baseMelee.Z },
                TowerPosition.RangedLeft => baseRanged,
                TowerPosition.RangedRight => baseRanged with { Z = 200f - baseRanged.Z },
                _ => baseMelee,
            };
            if (!C.IsGroup1) pos = pos with { X = 200f - pos.X, Z = 200f - pos.Z };
            return pos;
        }
        if (_phase is 10 or 11 && _phase11Sub == 0)
        {
            IGameObject? tower = GetShouldTakeTower(me);
            if (tower == null) return null;
            uint nameId = TowerKind(tower);
            bool isMelee = C.TowerPosition is TowerPosition.MeleeRight or TowerPosition.MeleeLeft;
            Vector3 p = tower.Position;
            if (nameId == (uint)Towers.DoomLight)
            {
                if (isMelee) p += new Vector3(0, 0, tower.Position.Z > 100 ? 1.5f : -1.5f);
                else p += new Vector3(tower.Position.X > 100 ? 1.5f : -1.5f, 0, 0);
            }
            else if (nameId == (uint)Towers.WindLight)
                p += new Vector3(tower.Position.X > 100 ? -1.5f : 1.5f, 0, 0);
            return p;
        }
        if (_phase is 10 or 11 && _phase11Sub == 1 && me.StatusList.Any(s => s.StatusId == 4768))
            return me.Position;
        return null;
    }

    private Vector3? OutsideShareSpot(IPlayerCharacter me)
    {
        if (!_clonePositions.TryGetValue(me.EntityId, out Vector3 clonePos)) return null;
        if (MathF.Abs(clonePos.X - 100f) > 8f) return clonePos;
        if (_playerOrder.TryGetValue(me.EntityId, out int ord) && (ord == 2 || ord == 6))
            return clonePos;
        return null;
    }

    private void UpdatePersonalGuide(IPlayerCharacter me)
    {
        if (C.SkipIndiMechs) { ClearGuide(); return; }

        Vector3? towerSpot = TowerGuideSpot(me);
        if (towerSpot.HasValue)
        {
            string towerLabel = _phase == 7 && _phase7Sub == 0 ? "SAFE" : "TOWER";
            Vector4 towerColor = _phase == 7 && _phase7Sub == 0 ? SafeColor : Guide;
            RefreshGuide(towerSpot, towerLabel, towerColor);
            return;
        }

        if (_phase is 15 or 16 or 17)
        {
            bool shareTime = _phase is 16 or 17 || (_phase == 15 && Adj() >= 5);
            if (shareTime)
            {
                Vector3? shareSpot = OutsideShareSpot(me);
                if (shareSpot.HasValue)
                {
                    string side = shareSpot.Value.X < 100f ? "WEST" : "EAST";
                    RefreshGuide(shareSpot, side, Guide);
                    return;
                }
            }
        }

        Vector3? spot = null;
        string label = "";
        Vector4 color = StackColor;

        if (_phase == 9 && Adj() < 4 && _defamationPlayers.TryGetValue(me.EntityId, out bool meDefa9))
        {
            int party = MyParty(me);
            int n = Adj();
            uint g2 = FindByOrder(n);
            uint g1 = FindByOrder(4 + n);
            bool defaOnYou = (g2 == me.EntityId && _defamationPlayers.TryGetValue(g2, out bool dg2) && dg2)
                          || (g1 == me.EntityId && _defamationPlayers.TryGetValue(g1, out bool dg1) && dg1);

            if (defaOnYou) { spot = DefaGroupPos(party); label = "DEFAMATION"; color = DefaColor; }
            else if (meDefa9) { spot = SafespotPos(party); label = "SAFE"; color = SafeColor; }
            else { spot = StackGroupPos(party); label = "STACK"; color = StackColor; }
        }
        else if ((_phase == 13 && Adj() < 5) || ((_phase is 16 or 17) && Adj() < 6))
        {
            int seqIdx = ReenactActiveOrderIndex();
            if (seqIdx >= 0)
            {
                var ordered = EnumCwKv(_clonePositions, new Vector2(100, 100), new Vector2(98, 86));
                if (seqIdx < ordered.Count && ordered[seqIdx].Key == me.EntityId
                    && _defamationPlayers.TryGetValue(me.EntityId, out bool meDefaR))
                {
                    if (meDefaR) { spot = DefaSpotFor(me); label = "DEFAMATION"; color = DefaColor; }
                    else { spot = _stackFinal; label = "STACK"; color = StackColor; }
                }
            }
        }

        if (label.Length == 0) { ClearGuide(); return; }
        RefreshGuide(spot, label, color);
    }

    private void RefreshGuide(Vector3? spot, string label, Vector4 color)
    {
        if (Plugin.Instance == null) return;

        bool showText = C.ShowGuideText;
        bool showPath = C.ShowGuidePath && spot.HasValue;
        bool spotSame = _lastGuideSpot.HasValue == spot.HasValue
            && (!spot.HasValue || Vector3.Distance(_lastGuideSpot!.Value, spot!.Value) < 0.05f);
        if (_guideLive && spotSame && _lastGuideLabel == label
            && _lastShowText == showText && _lastShowPath == showPath)
            return;

        _guideLive = true;
        _lastGuideSpot = spot;
        _lastGuideLabel = label;
        _lastShowText = showText;
        _lastShowPath = showPath;

        var e = new LogEvent { Name = "idyllic_guide" };
        Plugin.Instance.Engine.ClearExternal(GuideOwner);

        if (showText)
        {
            Plugin.Instance.Engine.SpawnExternal(GuideOwner, new DrawSpec
            {
                Shape = QuickShape.Text,
                Anchor = DrawAnchor.Self,
                AttachToActor = true,
                Color = color,
                Duration = 600f,
                Label = label,
                LabelColor = color,
                LabelSize = 1.2f,
                LabelHeight = 2f,
            }, e, previewSelf: true);
        }

        if (showPath)
        {
            Plugin.Instance.Engine.SpawnExternal(GuideOwner, new DrawSpec
            {
                Shape = QuickShape.ChevronPath,
                Anchor = DrawAnchor.Self,
                AttachToActor = true,
                Link = LinkTarget.FixedSpot,
                LinkPosition = spot!.Value,
                Color = color,
                ChevronSpacing = 2f,
                LineThickness = 4f,
                Length = 30f,
                Duration = 600f,
            }, e, previewSelf: true);
        }
    }

    private void ClearGuide()
    {
        if (!_guideLive) return;
        _guideLive = false;
        _lastGuideSpot = null;
        _lastGuideLabel = "";
        _lastShowText = false;
        _lastShowPath = false;
        Plugin.Instance?.Engine.ClearExternal(GuideOwner);
    }

    // safe platform (phases 12-15)
    private void SafePlatform()
    {
        if (_nextCleavesNorthSouth == null) return;
        bool eastUnsafe = _nextCleaves.Any(x => x.Pos.X < 100);
        bool mustGo = _phase > 13 || (_phase == 13 && Adj() >= 5);
        string side = eastUnsafe ? "West" : "East";
        string kind = _nextCleavesNorthSouth.Value ? "LeftRight" : "FrontBack";
        string key = $"Safe{side}{kind}{(mustGo ? "A" : "")}";
        ShowAt(key, SafePlatformPos(key), mustGo ? Guide : SafeColor);
    }

    // portal cones (phase 17)
    private void PortalCones()
    {
        if (_nextCleavesNorthSouth == true)
        {
            ShowCone("PortalConeNS1", new Vector3(100, 0, 92.5f), 0f, ConeColor);
            ShowCone("PortalConeNS2", new Vector3(100, 0, 92.5f), MathF.PI, ConeColor);
        }
        else if (_nextCleavesNorthSouth == false)
        {
            ShowCone("PortalConeEW1", new Vector3(100, 0, 92.5f), 3f * MathF.PI / 2f, ConeColor);
            ShowCone("PortalConeEW2", new Vector3(100, 0, 92.5f), MathF.PI / 2f, ConeColor);
        }
    }

    // generic cleave scan (phases 2/4)
    private void ScanCleaves(uint coneAction, float min, float max, uint aoeAction)
    {
        foreach (IGameObject o in Svc.Objects)
        {
            if (o is not IBattleChara bc || !bc.IsCasting) continue;
            float ct = bc.CurrentCastTime;
            if (bc.CastActionId == coneAction && ct >= min && ct <= max)
                _nextCleaves.Add((bc.Position, bc.Rotation));
            if (bc.CastActionId == aoeAction && (max > 900f || (ct >= min && ct <= max)))
                _nextAOE = bc.Position;
        }
    }

    private int Adj() => _defamationAttack / 2;

    private uint FindByOrder(int order)
    {
        foreach (var kv in _playerOrder)
            if (kv.Value == order) return kv.Key;
        return 0;
    }

    private static uint TowerKind(IGameObject o)
    {
        uint id = (o as IBattleNpc)?.NameId ?? 0u;
        if (id == 0u) id = o.BaseId;
        return id;
    }

    private List<IGameObject> GetBossClones()
    {
        return EnumCw(
            Svc.Objects.Where(o => TowerKind(o) == BossCloneNameId && _cloneTethers.ContainsKey(o.EntityId)),
            new Vector2(100, 100), new Vector2(96, 86));
    }

    private static Vector2 V2(Vector3 p) => new(p.X, p.Z);

    private static float AngleCw(Vector2 v)
    {
        float a = MathF.Atan2(v.X, -v.Y) * (180f / MathF.PI);
        if (a < 0f) a += 360f;
        return a;
    }

    private static List<IGameObject> EnumCw(IEnumerable<IGameObject> objs, Vector2 center, Vector2 start)
    {
        float s = AngleCw(start - center);
        return objs.OrderBy(o => Norm(AngleCw(V2(o.Position) - center) - s)).ToList();
    }

    private static List<KeyValuePair<uint, Vector3>> EnumCwKv(Dictionary<uint, Vector3> map, Vector2 center, Vector2 start)
    {
        float s = AngleCw(start - center);
        return map.OrderBy(kv => Norm(AngleCw(V2(kv.Value) - center) - s)).ToList();
    }

    private static float Norm(float a) => ((a % 360f) + 360f) % 360f;

    private static bool Approx(float a, float b, float tol) => MathF.Abs(a - b) <= tol;

    private static Vector3 SafespotPos(int party) => party == 1 ? new(98.5f, 0, 109f) : new(101f, 0, 109f);
    private static Vector3 DefaGroupPos(int g) => g == 1 ? new(86.234f, 0, 86.020f) : new(113.899f, 0, 86.115f);
    private static Vector3 StackGroupPos(int g) => g == 1 ? new(92f, 0, 100f) : new(108f, 0, 100f);

    private static Vector3 BaitPos(string name) => name switch
    {
        "Given Far" => new(110.152f, 0, 98.237f),
        "Given Near" => new(106.973f, 0, 94.048f),
        "Taken Far" => new(114.708f, 0, 109.144f),
        "Taken Near" => new(108.921f, 0, 92.552f),
        _ => new(100, 0, 100),
    };

    private static Vector3 SafePlatformPos(string key) => key switch
    {
        "SafeWestLeftRight" or "SafeWestLeftRightA" => new(85f, 0, 95f),
        "SafeEastLeftRight" or "SafeEastLeftRightA" => new(115f, 0, 95f),
        "SafeWestFrontBack" or "SafeWestFrontBackA" => new(92f, 0, 100f),
        "SafeEastFrontBack" or "SafeEastFrontBackA" => new(108f, 0, 100f),
        _ => new(100, 0, 100),
    };

    private Vector3 ElPos(string key) => _el.TryGetValue(key, out StaticVfx? e) && e.Position != Vector3.Zero ? e.Position : new Vector3(100, 0, 100);

    private void Build()
    {
        if (_built) return;
        _built = true;

        // "go here" guidance markers (circle + tether to player handled in code)
        foreach (string k in new[] { "DefamationGroup1", "DefamationGroup2", "StackGroup1", "StackGroup2",
            "SafespotGroup1", "SafespotGroup2", "Given Far", "Given Near", "Taken Far", "Taken Near",
            "p7sub1 tether", "DefamationOnYou" })
            MakeCircle(k, 1.0f, Guide);

        MakeCircle("TowerTether", 3.0f, Guide);
        MakeCircle("stack tether", 4.5f, Guide);
        MakeCircle("P7AOERadius", 6.3f, MeteorColor);

        MakeCircle("Rock1", 4f, TowerColor);
        MakeCircle("Rock2", 4f, TowerColor);
        MakeCircle("PickTetherCircle", 2.5f, Guide);

        foreach (string k in new[] { "SafeWestLeftRight", "SafeEastLeftRight", "SafeWestFrontBack", "SafeEastFrontBack",
            "SafeWestLeftRightA", "SafeEastLeftRightA", "SafeWestFrontBackA", "SafeEastFrontBackA" })
            MakeCircle(k, 3f, SafeColor);

        // danger telegraphs
        MakeDonut("Defamation1", 1f, 19f, DefaColor);
        MakeDonut("Defamation2", 1f, 19f, DefaColor);
        MakeCircle("Stack1", 4.5f, StackColor);
        MakeCircle("Stack2", 4.5f, StackColor);
        MakeCircle("Circle", 10f, MeteorColor);

        // tethers / lines
        MakeLine("PickTether", Guide);

        // cones (40 radius, 90deg)
        foreach (string k in new[] { "Cone1", "Cone2", "Cone3", "Cone4", "PortalConeNS1", "PortalConeNS2", "PortalConeEW1", "PortalConeEW2" })
            MakeCone(k, 40f, 90, ConeColor);
        // near/far cones (60 radius, 30deg)
        foreach (string k in new[] { "FarCone1", "FarCone2", "NearCone1", "NearCone2" })
            MakeCone(k, 60f, 30, ConeColor);
    }

    private void Add(string key, StaticVfx? v)
    {
        if (v == null) return;
        v.Enable = false;
        _el[key] = v;
        aoes.Add(v);
    }

    private void MakeCircle(string key, float radius, Vector4 color)
    {
        Add(key, DrawManager.Draw(new DrawElement
        {
            drawAvfx = "customCircle",
            radiusX = radius,
            radiusZ = radius,
            drawOnObject = false,
            Position = new Vector3(100, 0, 100),
            refColor = color,
            refTargetColor = color,
            destroyTime = 6000000f,
        }));
    }

    private void MakeDonut(string key, float inner, float outer, Vector4 color)
    {
        Add(key, DrawManager.Draw(new DrawElement
        {
            drawAvfx = "customDonut",
            radiusX = outer,
            radiusZ = outer,
            refRadian = inner / outer,
            drawOnObject = false,
            Position = new Vector3(100, 0, 100),
            refColor = color,
            refTargetColor = color,
            destroyTime = 6000000f,
        }));
    }

    private void MakeCone(string key, float length, int degree, Vector4 color)
    {
        Add(key, DrawManager.Draw(new DrawElement
        {
            drawAvfx = ShapeUtil.GetGameFanOmen(degree),
            refRadian = degree.Degrees().Rad,
            radiusX = length,
            radiusZ = length,
            fixRotation = true,
            drawOnObject = false,
            Position = new Vector3(100, 0, 100),
            refColor = color,
            refTargetColor = color,
            destroyTime = 6000000f,
        }));
    }

    private void MakeLine(string key, Vector4 color)
    {
        Add(key, DrawManager.Draw(new DrawElement
        {
            drawAvfx = "customRect",
            radiusX = 0.4f,
            radiusY = 1f,
            radiusZ = 1f,
            drawOnObject = false,
            endToTarget = true,
            Position = new Vector3(100, 0, 100),
            targetPosition = new Vector3(100, 0, 100),
            refColor = color,
            refTargetColor = color,
            destroyTime = 6000000f,
        }));
    }

    private void HideAll()
    {
        foreach (StaticVfx v in _el.Values) v.Enable = false;
    }

    private void ShowAt(string key, Vector3 pos, Vector4 color)
    {
        if (!_el.TryGetValue(key, out StaticVfx? e)) return;
        e.Position = pos;
        e.Color = color;
        e.TargetColor = color;
        e.Enable = true;
    }

    private void ShowLine(string key, Vector3 from, Vector3 to, Vector4 color)
    {
        if (!_el.TryGetValue(key, out StaticVfx? e)) return;
        e.Position = from;
        e.TargetPosition = to;
        e.Color = color;
        e.TargetColor = color;
        e.Enable = true;
    }

    private void ShowCone(string key, Vector3 pos, float rotRad, Vector4 color)
    {
        if (!_el.TryGetValue(key, out StaticVfx? e)) return;
        e.Position = pos;
        e.Rotation = rotRad.Radians();
        e.Color = color;
        e.TargetColor = color;
        e.Enable = true;
    }

    private void ShowConeBetween(string key, Vector3 from, Vector3 to)
    {
        Vector3 d = to - from;
        float rot = MathF.Atan2(d.X, d.Z);
        ShowCone(key, from, rot, ConeColor);
    }

    public override void Reset()
    {
        IsRunning = false;
        ClearGuide();
        base.Reset();
        _el.Clear();
        _built = false;
        _stackFinal = null;
        _phase = 0;
        _phase7Sub = 0;
        _phase11Sub = 0;
        _defamationAttack = 0;
        _playerPosition = -1;
        _isCardinalFirst = null;
        _isThDecreasingResistance = null;
        _isConeSafeNorth = null;
        _nextCleavesNorthSouth = null;
        _nextAOE = null;
        _nextCleaves.Clear();
        _clonePositions.Clear();
        _defamationPlayers.Clear();
        _playerOrder.Clear();
        _cloneTethers.Clear();
        _captureTowersAt = 0;
        _towersCaptured = false;
        _towers = MakeTowerArray();
    }

    private static Vector4 HsvToRgb(float h, float s, float v)
    {
        float r = 0, g = 0, b = 0;
        int i = (int)(h * 6f);
        float f = h * 6f - i;
        float p = v * (1f - s);
        float q = v * (1f - f * s);
        float t = v * (1f - (1f - f) * s);
        switch (i % 6)
        {
            case 0: r = v; g = t; b = p; break;
            case 1: r = q; g = v; b = p; break;
            case 2: r = p; g = v; b = t; break;
            case 3: r = p; g = q; b = v; break;
            case 4: r = t; g = p; b = v; break;
            case 5: r = v; g = p; b = q; break;
        }
        return new Vector4(r, g, b, 1f);
    }

    private static readonly string[] TowerNames = { "Melee Left", "Melee Right", "Ranged Left", "Ranged Right" };

    public override void DrawConfig()
    {
        EnsureEnableMigrated();
        bool active = ModuleConfig.IsEnabled(ModuleEnableKey);
        if (StratUI.Header("Idyllic Dream \u2014 Uptime (Tired)", ref active))
        {
            ModuleConfig.SetEnabled(ModuleEnableKey, active);
            C.Active = active;
            ModuleConfig.Save<Config>();
        }

        StratUI.Hint("Defaults are the tired/zenith uptime guide (defamations north, stacks E/W). Set your platform, tower and bait below.");

        StratUI.Section("Platform");
        int grp = C.IsGroup1 ? 0 : 1;
        if (StratUI.SegmentedBar(new[] { "West (LP1)", "East (LP2)" }, ref grp))
        {
            C.IsGroup1 = grp == 0;
            ModuleConfig.Save<Config>();
        }

        StratUI.Section("My tower position (looking at boss)");
        int tp = (int)C.TowerPosition;
        if (StratUI.SegmentedBar(TowerNames, ref tp))
        {
            C.TowerPosition = (TowerPosition)tp;
            ModuleConfig.Save<Config>();
        }

        StratUI.Section("Near / Far baits");
        int basis = C.TakenCheckConditionIsTakenTower ? 0 : 1;
        if (StratUI.SegmentedBar(new[] { "By taken tower", "By role" }, ref basis))
        {
            C.TakenCheckConditionIsTakenTower = basis == 0;
            ModuleConfig.Save<Config>();
        }
        if (C.TakenCheckConditionIsTakenTower)
        {
            ImGui.AlignTextToFramePadding();
            ImGui.TextDisabled("Wind (far) baited by:");
            ImGui.SameLine();
            int who = C.TakenFarIsEarth ? 0 : 1;
            if (StratUI.SegmentedBar(new[] { "Earth player", "Fire player" }, ref who))
            {
                C.TakenFarIsEarth = who == 0;
                ModuleConfig.Save<Config>();
            }
        }
        else
        {
            ImGui.AlignTextToFramePadding();
            ImGui.TextDisabled("Wind (far) baited by:");
            ImGui.SameLine();
            int who = C.TakenFarIsMelee ? 0 : 1;
            if (StratUI.SegmentedBar(new[] { "Earth/Fire melee", "Earth/Fire ranged" }, ref who))
            {
                C.TakenFarIsMelee = who == 0;
                ModuleConfig.Save<Config>();
            }
        }

        StratUI.Section("Show");
        bool line = C.ShowTetherLine;
        if (ImGui.Checkbox("Tether-pickup as line", ref line)) { C.ShowTetherLine = line; ModuleConfig.Save<Config>(); }
        bool circle = C.ShowTetherCircle;
        if (ImGui.Checkbox("Mark the clone to grab from", ref circle)) { C.ShowTetherCircle = circle; ModuleConfig.Save<Config>(); }
        bool gpath = C.ShowGuidePath;
        if (ImGui.Checkbox("Path to my defamation/stack spot", ref gpath)) { C.ShowGuidePath = gpath; ModuleConfig.Save<Config>(); }
        bool gtext = C.ShowGuideText;
        if (ImGui.Checkbox("Callout text over me (DEFAMATION / STACK)", ref gtext)) { C.ShowGuideText = gtext; ModuleConfig.Save<Config>(); }
        bool noTowers = C.DontShowElementsP11S1;
        if (ImGui.Checkbox("Don't visualise tower debuffs (cones/tethers)", ref noTowers)) { C.DontShowElementsP11S1 = noTowers; ModuleConfig.Save<Config>(); }
        StratUI.Hint("Hides the wind/doom tower cone + tether resolution (phase 11). Other AoEs still show.");
        bool skip = C.SkipIndiMechs;
        if (ImGui.Checkbox("Don't resolve individual mechanics (only show danger AoEs)", ref skip)) { C.SkipIndiMechs = skip; ModuleConfig.Save<Config>(); }
        StratUI.Hint("Shows stack/defamation/stored AoEs only. Won't point you to your tether, your stack/spread spot, or your tower.");

        StratUI.Section("Preview");
        bool preview = C.Preview;
        if (ImGui.Checkbox("Preview fixed positions in arena (out of combat)", ref preview)) { C.Preview = preview; ModuleConfig.Save<Config>(); }
        if (C.Preview)
            StratUI.Hint("Drops the configured stand-here markers in the arena: defamations (purple), stacks (green), safe spots, and the four near/far baits. Live clone/tether/player resolution only renders during the real pull.");

        if (ImGui.CollapsingHeader("Debug"))
        {
            ImGui.TextUnformatted($"Phase {_phase}  sub7 {_phase7Sub}  sub11 {_phase11Sub}  defa {Adj()}");
            ImGui.TextUnformatted($"PlayerPos {_playerPosition}  CardinalFirst {(_isCardinalFirst?.ToString() ?? "?")}");
            ImGui.TextUnformatted($"ConeSafeNorth {(_isConeSafeNorth?.ToString() ?? "?")}  THlight {(_isThDecreasingResistance?.ToString() ?? "?")}");
            ImGui.TextUnformatted($"NS cleaves {(_nextCleavesNorthSouth?.ToString() ?? "?")}  cleaves {_nextCleaves.Count}  towers {(_towersCaptured ? "set" : "no")}");
            ImGui.TextUnformatted($"Clone tethers {_cloneTethers.Count}  positions {_clonePositions.Count}  order {_playerOrder.Count}");
        }
    }
}
