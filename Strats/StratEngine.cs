using System;
using System.Collections.Generic;
using System.Numerics;
using Dalamud.Plugin.Services;
using YapYapDraw.Logging;
using YapYapDraw.QuickDraws;

namespace YapYapDraw.Strats;

public sealed class StratEngine
{
    private const double SuppressSeconds = 2.5;

    private readonly Configuration    _config;
    private readonly QuickDrawEngine  _draw;
    private readonly IPluginLog       _log;
    private readonly CombatLogCapture _capture;

    private readonly Dictionary<string, DateTime> _lastFire     = new();
    private readonly Dictionary<string, string>   _manualBranch = new();
    private readonly List<Pending>                 _pending     = new();

    private readonly struct Pending
    {
        public Pending(DateTime when, StratSlide slide, RoleSpot spot, LogEvent e)
        { When = when; Slide = slide; Spot = spot; Event = e; }
        public DateTime   When  { get; }
        public StratSlide Slide { get; }
        public RoleSpot   Spot  { get; }
        public LogEvent   Event { get; }
    }

    public StratEngine(Configuration config, QuickDrawEngine draw, IPluginLog log, CombatLogCapture capture)
    {
        _config  = config;
        _draw    = draw;
        _log     = log;
        _capture = capture;
    }

    public void SetManualBranch(string slideId, string branchId) => _manualBranch[slideId] = branchId;
    public string? GetManualBranch(string slideId) => _manualBranch.TryGetValue(slideId, out var b) ? b : null;

    public StratPack? ActivePack()
    {
        if (!_config.StratsEnabled) return null;
        uint terr = Plugin.ClientState.TerritoryType;
        var matches = _config.StratPacks.FindAll(p => p.Enabled && p.Territory == terr);
        if (matches.Count == 0) return null;

        if (_config.SelectedStrat.TryGetValue(terr.ToString(), out var id))
        {
            var sel = matches.Find(p => p.Id == id);
            if (sel != null) return sel;
        }
        return matches[0];
    }

    public void Handle(LogEvent e)
    {
        var pack = ActivePack();
        if (pack == null) return;

        foreach (var slide in pack.Slides)
        {
            if (!SlideMatches(slide, e)) continue;

            if (_lastFire.TryGetValue(slide.Id, out var last) &&
                (DateTime.Now - last).TotalSeconds < SuppressSeconds)
                continue;
            _lastFire[slide.Id] = DateTime.Now;

            var branch = ResolveBranch(slide, e);
            if (branch == null) continue;

            var spot = branch.Spots.Find(s => s.Enabled && s.Role == _config.MyRole);
            if (spot == null) continue;

            if (slide.DelaySeconds > 0.01f)
                _pending.Add(new Pending(DateTime.Now.AddSeconds(slide.DelaySeconds), slide, spot, e));
            else
                FireSpot(slide, spot, e);
        }
    }

    public void Tick()
    {
        if (_pending.Count == 0) return;
        var now = DateTime.Now;
        for (int i = _pending.Count - 1; i >= 0; i--)
        {
            if (_pending[i].When > now) continue;
            var p = _pending[i];
            _pending.RemoveAt(i);
            FireSpot(p.Slide, p.Spot, p.Event);
        }
    }

    public void Preview(StratSlide slide, StratBranch branch)
    {
        var spot = branch.Spots.Find(s => s.Enabled && s.Role == _config.MyRole)
                   ?? (branch.Spots.Count > 0 ? branch.Spots[0] : null);
        if (spot == null) return;
        FireSpot(slide, spot, new LogEvent { Name = "preview" }, preview: true);
    }

    private void FireSpot(StratSlide slide, RoleSpot spot, LogEvent e, bool preview = false)
    {
        string markerKey = $"strat:{slide.Id}:marker";
        string leashKey  = $"strat:{slide.Id}:leash";

        _draw.ClearExternal(markerKey);
        _draw.ClearExternal(leashKey);

        bool dynamic = spot.Anchor == SpotAnchor.TetheredToMe;

        var marker = new DrawSpec
        {
            Shape            = spot.Shape,
            Color            = spot.Color,
            Radius           = spot.Radius,
            InnerRadius      = MathF.Max(0.1f, spot.Radius - 1f),
            HalfWidth        = MathF.Max(0.5f, spot.Radius),
            Anchor           = dynamic ? DrawAnchor.TetheredToMe : DrawAnchor.FixedPosition,
            AttachToActor    = dynamic,
            FixedPosition    = spot.Position,
            TetherFilterId   = spot.TetherId,
            Duration         = spot.Duration,
            UseEventDuration = false,
        };
        _draw.SpawnExternal(markerKey, marker, e, preview);

        if (spot.ShowLeash)
        {
            var leash = new DrawSpec
            {
                Shape          = QuickShape.ChevronPath,
                Color          = spot.LeashColor,
                ChevronSpacing = 2.5f,
                LineThickness  = 4f,
                Anchor         = DrawAnchor.Self,
                AttachToActor  = true,
                Link           = dynamic ? LinkTarget.TetheredToMe : LinkTarget.FixedSpot,
                LinkPosition   = spot.Position,
                TetherFilterId = spot.TetherId,
                Duration       = spot.Duration,
            };
            _draw.SpawnExternal(leashKey, leash, e, preview);
        }
    }

    private StratBranch? ResolveBranch(StratSlide slide, LogEvent e)
    {
        if (slide.Branches.Count == 0) return null;
        if (slide.Branches.Count == 1) return slide.Branches[0];

        StratBranch? catchAll = null;

        // First-match-wins, top to bottom. Put specific variants above the default.
        foreach (var b in slide.Branches)
        {
            if (b.Conditions.Count > 0)
            {
                if (EvalBranch(b, e)) return b;
                continue;
            }

            switch (b.Detect)
            {
                case BranchDetect.MyStatus:
                    if (b.StatusId != 0 && SelfHasStatus(b.StatusId)) return b;
                    break;
                case BranchDetect.BossPosition:
                    var side = BossSideFor(b.BossId, e);
                    if (side != null && b.BossSide == side.Value) return b;
                    break;
                default:
                    catchAll ??= b;
                    break;
            }
        }

        if (_manualBranch.TryGetValue(slide.Id, out var bid))
        {
            var sel = slide.Branches.Find(b => b.Id == bid);
            if (sel != null) return sel;
        }
        return catchAll ?? slide.Branches[0];
    }

    private bool EvalBranch(StratBranch b, LogEvent e)
    {
        if (b.RequireAll)
        {
            foreach (var c in b.Conditions)
                if (!EvalCondition(c, e)) return false;
            return true;
        }
        foreach (var c in b.Conditions)
            if (EvalCondition(c, e)) return true;
        return false;
    }

    private bool EvalCondition(StratCondition c, LogEvent e)
    {
        bool ok = c.Kind switch
        {
            CondKind.MyStatus   => c.StatusId != 0 && SelfHasStatus(c.StatusId),
            CondKind.MyRole     => SelfRoleMatches(c.Role),
            CondKind.BossSide   => BossSideFor(c.BossId, e) is Compass s && s == c.BossSide,
            CondKind.TetherOnMe => SelfHasTether(c.TetherId),
            _                   => true,
        };
        return c.Negate ? !ok : ok;
    }

    private static bool SelfHasStatus(uint statusId)
    {
        var me = Plugin.ObjectTable.LocalPlayer;
        if (me == null) return false;
        foreach (var s in me.StatusList)
            if (s != null && s.StatusId == statusId) return true;
        return false;
    }

    private static bool SelfRoleMatches(RoleCat want)
    {
        var me = Plugin.ObjectTable.LocalPlayer;
        byte role = me?.ClassJob.ValueNullable?.Role ?? 0; // 1 tank, 2 melee, 3 ranged, 4 healer
        return want switch
        {
            RoleCat.Tank   => role == 1,
            RoleCat.Healer => role == 4,
            RoleCat.Melee  => role == 2,
            RoleCat.Ranged => role == 3,
            RoleCat.Dps    => role is 2 or 3,
            _              => false,
        };
    }

    private bool SelfHasTether(uint tetherId)
    {
        var me = Plugin.ObjectTable.LocalPlayer;
        if (me == null) return false;
        uint id = me.EntityId;
        foreach (var t in _capture.ActiveTethers)
            if ((tetherId == 0 || t.Id == tetherId) && (t.From == id || t.To == id))
                return true;
        return false;
    }

    private static Compass? BossSideFor(uint bossId, LogEvent e)
    {
        Dalamud.Game.ClientState.Objects.Types.IGameObject? boss = null;
        if (bossId != 0) boss = Plugin.ObjectTable.SearchById(bossId);
        if (boss == null && e.SourceId != 0)
            boss = Plugin.ObjectTable.SearchById(e.SourceId);
        if (boss == null) return null;

        float dx = boss.Position.X - 100f;
        float dz = boss.Position.Z - 100f;
        if (MathF.Abs(dx) < 0.5f && MathF.Abs(dz) < 0.5f) return null;

        float ang = MathF.Atan2(dx, -dz) * (180f / MathF.PI);
        if (ang < 0) ang += 360f;
        int oct = (int)MathF.Round(ang / 45f) & 7;
        return (Compass)oct;
    }

    private static bool SlideMatches(StratSlide slide, LogEvent e)
    {
        bool kindOk = slide.On switch
        {
            TriggerMatch.Any        => true,
            TriggerMatch.Cast       => e.Kind == LogKind.CastStart,
            TriggerMatch.CastEnd    => e.Kind is LogKind.CastFinish or LogKind.Ability,
            TriggerMatch.StatusGain => e.Kind == LogKind.StatusGain,
            TriggerMatch.StatusLose => e.Kind == LogKind.StatusLose,
            TriggerMatch.Death      => e.Kind == LogKind.Death,
            TriggerMatch.Headmarker => e.Kind == LogKind.Headmarker,
            TriggerMatch.Tether     => e.Kind == LogKind.Tether,
            TriggerMatch.Chat       => e.Kind == LogKind.Chat,
            _                       => false,
        };
        if (!kindOk) return false;
        if (slide.MatchById) return e.DataId == slide.DataId;
        if (string.IsNullOrEmpty(slide.Pattern)) return true;
        return e.Name.Contains(slide.Pattern, StringComparison.OrdinalIgnoreCase);
    }
}
