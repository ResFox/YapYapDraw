using System;
using System.Collections.Generic;
using System.Globalization;
using System.Numerics;
using System.Text.RegularExpressions;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Plugin.Services;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Enum;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.Util;
using YapYapDraw.Engine.Vfx;
using YapYapDraw.Logging;

namespace YapYapDraw.QuickDraws;

// Event -> match -> draw floor shape.
public sealed class QuickDrawEngine
{
    private const double SuppressSeconds = 2.5;

    private readonly Configuration _config;
    private readonly IPluginLog    _log;
    private readonly Dictionary<string, Regex>    _regexCache = new();
    private readonly Dictionary<string, DateTime> _lastFire   = new();
    private readonly Dictionary<string, string>   _vars       = new(StringComparer.OrdinalIgnoreCase);
    private readonly List<(DateTime when, QuickDrawDef t, LogEvent e, string key)> _pending = new();
    private readonly List<(DateTime expiry, QuickDrawDef t, string key, uint subject)> _clearWatch = new();

    private readonly List<(DateTime when, FollowUpStep s, LogEvent ctx, string key)> _pendingFollow = new();
    private readonly List<ArmedFollow> _armedFollow = new();

    // What live event a drawn shape is pinned to, so it can vanish on the exact
    // frame the cast resolves / the debuff falls off instead of guessing a timer.
    private enum BindKind : byte { None, Cast, Status }

    // Live shapes per instance key, used for Replace/Wait concurrency and ClearOn.
    // The key folds in the affected actor so the same draw can light up every
    // player a mechanic lands on at once, each cleared independently.
    private sealed class Tracked
    {
        public StaticVfx Vfx = null!;
        public DateTime  Expiry;
        public BindKind  Bind;
        public uint      BindSrc;
        public uint      BindId;
    }
    private readonly Dictionary<string, List<Tracked>> _live = new();

    private sealed class ActiveCast { public float Duration; public DateTime Ends; }
    private readonly Dictionary<(uint actor, uint action), ActiveCast> _activeCasts = new();

    private sealed class ArmedFollow
    {
        public FollowUpStep Step    = null!;
        public LogEvent     Ctx     = null!;
        public DateTime     Expiry;
        public bool[]       Met     = Array.Empty<bool>();
        public LogEvent?    Trigger;
        public string       Key     = "";
    }

    public QuickDrawEngine(Configuration config, IPluginLog log)
    {
        _config = config;
        _log    = log;
    }

    public void Handle(LogEvent e)
    {
        if (!_config.QuickDrawsEnabled) return;

        TrackCast(e);
        ReleaseBound(e);
        ProcessArmed(e);
        ProcessClearWatch(e);

        foreach (var m in _config.QuickDrawModules)
        {
            if (!m.Enabled) continue;
            foreach (var t in m.Draws)
            {
                if (!t.Enabled) continue;
                if (!Matches(t, e)) continue;

                uint   subject = TriggerSubject(t, e);
                string key     = InstanceKey(t.Id, subject);

                double cd = t.Cooldown > 0.01f ? t.Cooldown : SuppressSeconds;
                if (_lastFire.TryGetValue(key, out var last) &&
                    (DateTime.Now - last).TotalSeconds < cd)
                    continue;

                if (ModeOf(t) == Concurrency.Wait && OwnerLive(key)) continue;

                _lastFire[key] = DateTime.Now;

                ApplyVars(t, e);

                if (t.DelaySeconds > 0.01f)
                    _pending.Add((DateTime.Now.AddSeconds(t.DelaySeconds), t, e, key));
                else
                    Fire(t, e, key);

                ArmClear(t, key, subject);
            }
        }
    }

    public void Fire(QuickDrawDef t, LogEvent e) => Fire(t, e, InstanceKey(t.Id, TriggerSubject(t, e)));

    private void Fire(QuickDrawDef t, LogEvent e, string key)
    {
        if (ModeOf(t) == Concurrency.Replace) ClearOwner(key);

        if (t.DrawEnabled)
            SpawnShape(key, t.Draw, e);

        var now = DateTime.Now;
        foreach (var s in t.FollowUps)
        {
            string stepKey = InstanceKey(s.Id, EventSubject(e));
            if (s.On == FollowUpOn.Timer)
            {
                _pendingFollow.Add((now.AddSeconds(Math.Max(0f, s.Seconds)), s, e, stepKey));
                continue;
            }

            s.EnsureConditions();
            var armed = new ArmedFollow
            {
                Step   = s,
                Ctx    = e,
                Expiry = now.AddSeconds(Math.Max(0.1f, s.Seconds)),
                Met    = new bool[Math.Max(1, s.Conditions.Count)],
                Key    = stepKey,
            };

            if (!TryAdvance(armed, e))
                _armedFollow.Add(armed);
        }
    }

    private void FireStep(FollowUpStep s, LogEvent ctx, string key)
    {
        if (s.DrawEnabled)
            SpawnShape(key, s.Draw, ctx);
    }

    private static string InstanceKey(string id, uint subject)
        => subject == 0 ? id : id + "#" + subject;

    // The actor a fired draw belongs to, so simultaneous hits on several players
    // each get their own shape and cooldown instead of stomping one another.
    private static uint TriggerSubject(QuickDrawDef t, LogEvent e) => t.On switch
    {
        TriggerMatch.StatusGain or TriggerMatch.StatusLose => e.TargetId,
        TriggerMatch.Headmarker => e.SourceId,
        TriggerMatch.Tether     => e.TargetId != 0 ? e.TargetId : e.SourceId,
        TriggerMatch.Cast or TriggerMatch.CastEnd or TriggerMatch.Death => e.SourceId,
        _ => e.TargetId != 0 ? e.TargetId : e.SourceId,
    };

    private static uint EventSubject(LogEvent e)
        => e.TargetId != 0 ? e.TargetId : e.SourceId;

    public void Preview(QuickDrawDef t)
    {
        var sample = new LogEvent { Name = string.IsNullOrEmpty(t.Pattern) ? "Sample" : t.Pattern };
        if (t.DrawEnabled)
        {
            ClearOwner(t.Id);
            SpawnShape(t.Id, t.Draw, sample, previewSelf: true);
        }
    }

    private void SpawnShape(string ownerId, DrawSpec d, LogEvent e, bool previewSelf = false)
    {
        Vector3? pos = ResolvePosition(d, e, previewSelf, out IGameObject? attach);
        if (pos == null && attach == null) return;

        float life = ResolveEventLife(d, e);
        float ms   = Math.Max(0.1f, life) * 1000f;
        bool  glue = d.AttachToActor && attach != null;

        // Spinning with the actor only works while glued to one.
        bool faceActor = d.OrientToFacing && glue;

        var elem = new DrawElement
        {
            Position     = pos ?? (attach != null ? attach.Position : new Vector3(100, 0, 100)),
            drawOnObject = glue,
            refColor       = d.Color,
            refTargetColor = d.Color,
            destroyTime  = ms,
            // Local frame: forward is -Z, right is -X after the engine's rotate+subtract.
            refOffsetZ = -d.OffsetForward,
            refOffsetX = -d.OffsetSide,
        };

        // A tether needs a live actor to grow out of; if the anchor isn't an actor,
        // start it from the player so it still points at the far end.
        IGameObject? owner = glue ? attach : null;
        if (d.Shape == QuickShape.Line && owner == null)
        {
            owner = Actor(Plugin.PlayerState.EntityId);
            glue  = owner != null;
        }

        if (!ApplyLegacyCustom(elem, d, faceActor))
        switch (d.Shape)
        {
            case QuickShape.Circle:
                elem.drawAvfx = "customCircle";
                elem.radiusX = d.Radius; elem.radiusZ = d.Radius;
                break;
            case QuickShape.Donut:
            {
                elem.drawAvfx = "customDonut";
                float outer = MathF.Max(d.Radius, d.InnerRadius + 0.1f);
                float inner = MathF.Min(d.InnerRadius, outer - 0.1f);
                elem.radiusX = outer; elem.radiusZ = outer;
                elem.refRadian = inner / outer;
                break;
            }
            case QuickShape.Fan:
                elem.drawAvfx = ShapeUtil.GetGameFanOmen(d.FanAngle);
                elem.refRadian = d.FanAngle.Degrees().Rad;
                elem.radiusX = d.Radius; elem.radiusZ = d.Radius;
                ApplyRotation(elem, d, faceActor);
                break;
            case QuickShape.Rectangle:
                elem.drawAvfx = "customRect";
                elem.radiusX = d.HalfWidth;
                elem.radiusZ = d.Length;
                ApplyRotation(elem, d, faceActor);
                break;
            case QuickShape.Line:
                SetupLine(elem, d, e, attach, glue);
                break;
            case QuickShape.Tower:
                elem.drawAvfx = GroundOmen.SingleTower;
                elem.radiusX = d.Radius; elem.radiusZ = d.Radius; elem.radiusY = 1f;
                break;
            case QuickShape.Knockback:
                elem.drawAvfx = GroundOmen.KnockBack;
                elem.radiusX = d.Radius; elem.radiusZ = d.Radius; elem.radiusY = 1f;
                ApplyRotation(elem, d, faceActor);
                break;
            case QuickShape.Laser:
                elem.drawAvfx = GroundOmen.ArrowRect;
                elem.radiusX = d.HalfWidth;
                elem.radiusZ = d.Length;
                elem.radiusY = 1f;
                ApplyRotation(elem, d, faceActor);
                break;
        }

        var vfx = DrawManager.Draw(elem, glue ? owner : null);
        if (vfx == null) return;

        if (!_live.TryGetValue(ownerId, out var list)) { list = new(); _live[ownerId] = list; }

        var tracked = new Tracked { Vfx = vfx, Expiry = DateTime.Now.AddSeconds(life) };
        if (d.UseEventDuration)
        {
            // Pin the shape to the live event so it dies the instant the snapshot
            // lands, not a frame early or late from a guessed timer.
            if (e.Kind == LogKind.CastStart && e.SourceId != 0)
            { tracked.Bind = BindKind.Cast; tracked.BindSrc = e.SourceId; tracked.BindId = e.DataId; }
            else if (e.Kind == LogKind.StatusGain && e.TargetId != 0)
            { tracked.Bind = BindKind.Status; tracked.BindSrc = e.TargetId; tracked.BindId = e.DataId; }
        }
        list.Add(tracked);
    }

    // A bound shape is cleared the moment its cast resolves (CastFinish / the
    // resolving ability) or its debuff falls off (StatusLose).
    private void ReleaseBound(LogEvent e)
    {
        if (e.Kind is LogKind.CastFinish or LogKind.Ability)
            RemoveBound(BindKind.Cast, e.SourceId, e.DataId);
        else if (e.Kind == LogKind.StatusLose)
            RemoveBound(BindKind.Status, e.TargetId, e.DataId);
    }

    private void RemoveBound(BindKind kind, uint src, uint id)
    {
        if (src == 0 || id == 0) return;
        foreach (var list in _live.Values)
            for (int i = list.Count - 1; i >= 0; i--)
            {
                var tr = list[i];
                if (tr.Bind != kind || tr.BindSrc != src || tr.BindId != id) continue;
                try { tr.Vfx.Remove(); } catch { }
                list.RemoveAt(i);
            }
    }

    private static void ApplyRotation(DrawElement elem, DrawSpec d, bool faceActor)
    {
        if (faceActor)
        {
            elem.fixRotation  = false;        // add to the owner's heading each frame
            elem.refRotation  = d.Rotation.Degrees();
        }
        else
        {
            elem.fixRotation  = true;         // pinned to a world bearing
            elem.refRotation  = d.Rotation.Degrees();
        }
    }

    private static bool ApplyLegacyCustom(DrawElement elem, DrawSpec d, bool faceActor)
    {
        if (string.IsNullOrWhiteSpace(d.CustomVfx)) return false;
        elem.drawAvfx = d.CustomVfx.Trim();
        elem.radiusY  = 1f;
        elem.radiusX  = d.HalfWidth > 0.1f ? d.HalfWidth : d.Radius;
        elem.radiusZ  = d.Length > 0.1f ? d.Length : d.Radius;
        ApplyRotation(elem, d, faceActor);
        return true;
    }

    private void TrackCast(LogEvent e)
    {
        if (e.Kind == LogKind.CastStart && e.Value > 0.05f)
        {
            _activeCasts[(e.SourceId, e.DataId)] = new ActiveCast
            {
                Duration = e.Value,
                Ends     = DateTime.Now.AddSeconds(e.Value),
            };
            return;
        }

        if (e.Kind == LogKind.CastFinish)
            _activeCasts.Remove((e.SourceId, e.DataId));
    }

    private float ResolveEventLife(DrawSpec d, LogEvent e)
    {
        if (!d.UseEventDuration) return d.Duration;

        if (e.Value > 0.1f)
            return RemainingCastLife(e);

        if (e.Kind is LogKind.CastStart or LogKind.Ability)
            return LookupCastLife(e.SourceId, e.DataId) ?? d.Duration;

        if (e.Kind == LogKind.StatusGain)
            return LookupStatusLife(e.TargetId, e.DataId) ?? d.Duration;

        return d.Duration;
    }

    private float RemainingCastLife(LogEvent e)
    {
        if (_activeCasts.TryGetValue((e.SourceId, e.DataId), out var tracked))
        {
            var rem = (float)(tracked.Ends - DateTime.Now).TotalSeconds;
            if (rem > 0.1f) return rem;
        }

        if (Plugin.ObjectTable.SearchById(e.SourceId) is IBattleChara bc &&
            bc.IsCasting && bc.CastActionId == e.DataId)
        {
            var rem = bc.TotalCastTime - bc.CurrentCastTime;
            if (rem > 0.1f) return rem;
        }

        return e.Value;
    }

    private float? LookupCastLife(uint actorId, uint actionId)
    {
        if (_activeCasts.TryGetValue((actorId, actionId), out var tracked))
        {
            var rem = (float)(tracked.Ends - DateTime.Now).TotalSeconds;
            if (rem > 0.1f) return rem;
            if (tracked.Duration > 0.1f) return tracked.Duration;
        }

        if (Plugin.ObjectTable.SearchById(actorId) is IBattleChara bc &&
            bc.IsCasting && bc.CastActionId == actionId)
        {
            var rem = bc.TotalCastTime - bc.CurrentCastTime;
            if (rem > 0.1f) return rem;
        }

        return null;
    }

    private static float? LookupStatusLife(uint actorId, uint statusId)
    {
        if (actorId == 0 || statusId == 0) return null;
        if (Plugin.ObjectTable.SearchById(actorId) is not IBattleChara bc) return null;
        foreach (var s in bc.StatusList)
        {
            if (s.StatusId != statusId) continue;
            if (s.RemainingTime > 0.1f) return s.RemainingTime;
            break;
        }
        return null;
    }

    // A tether: a thin rect glued to the origin actor that stretches to the far end
    // (a spot, or another actor) and re-points at it each frame.
    private void SetupLine(DrawElement elem, DrawSpec d, LogEvent e, IGameObject? anchor, bool glue)
    {
        elem.drawAvfx     = "customRect";
        elem.radiusX      = MathF.Max(0.1f, d.HalfWidth);
        elem.radiusY      = 1f;
        elem.radiusZ      = 1f;          // base length; endToTarget scales it to reach
        elem.endToTarget  = true;
        elem.drawOnObject = glue;

        var far = ResolveLink(d, e, anchor);
        if (far != null)
            elem.target = far;
        else
            elem.targetPosition = d.Link == LinkTarget.FixedSpot
                ? d.LinkPosition
                : new Vector3(e.X, 0f, e.Y);
    }

    private IGameObject? ResolveLink(DrawSpec d, LogEvent e, IGameObject? anchor)
    {
        switch (d.Link)
        {
            case LinkTarget.EventTarget: return Actor(e.TargetId);
            case LinkTarget.EventSource: return Actor(e.SourceId);
            case LinkTarget.MyTarget:
                return Actor(Plugin.PlayerState.EntityId)?.TargetObject;
            case LinkTarget.NearestPlayer:
                return Nearest(anchor, onlyPlayers: true, wantEnemy: false);
            case LinkTarget.NearestEnemy:
                return Nearest(anchor, onlyPlayers: false, wantEnemy: true);
            case LinkTarget.PlayerWithSameStatus:
                return PlayerWithStatus(anchor, e.DataId);
            case LinkTarget.FixedSpot:
            default:
                return null;
        }
    }

    private static IGameObject? Nearest(IGameObject? from, bool onlyPlayers, bool wantEnemy)
    {
        if (from == null) return null;
        IGameObject? best = null;
        float bestSq = float.MaxValue;
        foreach (var o in Plugin.ObjectTable)
        {
            if (o.EntityId == from.EntityId) continue;
            bool isPc  = o.ObjectKind == Dalamud.Game.ClientState.Objects.Enums.ObjectKind.Pc;
            bool isNpc = o is IBattleNpc;
            if (onlyPlayers && !isPc) continue;
            if (wantEnemy && !isNpc) continue;
            if (o is not IBattleChara bc || bc.CurrentHp == 0) continue;
            float dSq = Vector3.DistanceSquared(o.Position, from.Position);
            if (dSq < bestSq) { bestSq = dSq; best = o; }
        }
        return best;
    }

    private static IGameObject? PlayerWithStatus(IGameObject? exclude, uint statusId)
    {
        if (statusId == 0) return null;
        foreach (var o in Plugin.ObjectTable)
        {
            if (exclude != null && o.EntityId == exclude.EntityId) continue;
            if (o.ObjectKind != Dalamud.Game.ClientState.Objects.Enums.ObjectKind.Pc) continue;
            if (o is not IBattleChara bc) continue;
            foreach (var s in bc.StatusList)
                if (s.StatusId == statusId) return o;
        }
        return null;
    }

    private Vector3? ResolvePosition(DrawSpec d, LogEvent e, bool previewSelf, out IGameObject? attach)
    {
        attach = null;

        // A preview has no real event actors, so everything but a fixed spot falls
        // back to the player. The fixed spot carries a real coordinate already, so
        // honour it — otherwise "test" would always snap the shape onto you.
        if (previewSelf && d.Anchor != DrawAnchor.FixedPosition)
        {
            var me = Actor(Plugin.PlayerState.EntityId);
            if (me != null) { attach = me; return me.Position; }
            return new Vector3(100, 0, 100);
        }

        switch (d.Anchor)
        {
            case DrawAnchor.Source:
                attach = Actor(e.SourceId);
                return attach?.Position;
            case DrawAnchor.Target:
                attach = Actor(e.TargetId);
                return attach?.Position;
            case DrawAnchor.Self:
                attach = Actor(Plugin.PlayerState.EntityId);
                return attach?.Position;
            case DrawAnchor.EventPosition:
                return new Vector3(e.X, 0f, e.Y);
            case DrawAnchor.FixedPosition:
            default:
                return d.FixedPosition;
        }
    }

    private static IGameObject? Actor(uint id)
        => id == 0 ? null : Plugin.ObjectTable.SearchById(id);

    private bool OwnerLive(string id)
    {
        PruneOwner(id);
        return _live.TryGetValue(id, out var list) && list.Count > 0;
    }

    private void ClearOwner(string id)
    {
        if (!_live.TryGetValue(id, out var list)) return;
        foreach (var t in list)
            try { t.Vfx.Remove(); } catch { }
        list.Clear();
    }

    private void PruneOwner(string id)
    {
        if (!_live.TryGetValue(id, out var list)) return;
        var now = DateTime.Now;
        list.RemoveAll(t => t.Expiry <= now);
    }

    private void ProcessArmed(LogEvent e)
    {
        if (_armedFollow.Count == 0) return;
        var now = DateTime.Now;
        for (int i = _armedFollow.Count - 1; i >= 0; i--)
        {
            var a = _armedFollow[i];
            if (now > a.Expiry) { _armedFollow.RemoveAt(i); continue; }
            if (TryAdvance(a, e)) _armedFollow.RemoveAt(i);
        }
    }

    private bool TryAdvance(ArmedFollow a, LogEvent e)
    {
        if (!KindMatches(a.Step.On, e)) return false;

        var conds = a.Step.Conditions;
        bool any = false;
        for (int c = 0; c < conds.Count; c++)
        {
            if (a.Met[c]) continue;
            if (!CondMatches(a.Step.On, conds[c], e)) continue;
            a.Met[c] = true;
            any = true;
            a.Trigger ??= e;
        }
        if (conds.Count == 0 && !any)
        {
            a.Trigger = e; a.Met = new[] { true }; any = true;
        }

        if (!any) return false;

        bool done = !a.Step.RequireAll || AllMet(a.Met);
        if (!done) return false;

        FireStep(a.Step, a.Trigger ?? e, a.Key);
        return true;
    }

    private static bool AllMet(bool[] met)
    {
        foreach (var m in met) if (!m) return false;
        return true;
    }

    private static bool KindMatches(FollowUpOn on, LogEvent e) => on switch
    {
        FollowUpOn.Cast       => e.Kind == LogKind.CastStart,
        FollowUpOn.CastEnd    => e.Kind is LogKind.CastFinish or LogKind.Ability,
        FollowUpOn.StatusGain => e.Kind == LogKind.StatusGain,
        FollowUpOn.StatusLose => e.Kind == LogKind.StatusLose,
        FollowUpOn.Headmarker => e.Kind == LogKind.Headmarker,
        FollowUpOn.Tether     => e.Kind == LogKind.Tether,
        FollowUpOn.Death      => e.Kind == LogKind.Death,
        FollowUpOn.Chat       => e.Kind == LogKind.Chat,
        _                     => false,
    };

    private bool CondMatches(FollowUpOn on, FollowCond c, LogEvent e)
    {
        if (on == FollowUpOn.Chat)
        {
            if (string.IsNullOrWhiteSpace(c.Pattern)) return true;
            if (c.UseRegex) return RegexMatch(c.Pattern, e.Name);
            return e.Name.Contains(c.Pattern, StringComparison.OrdinalIgnoreCase);
        }

        if (c.Source != SourceFilter.Anyone)
        {
            var want = c.Source switch
            {
                SourceFilter.Enemy => ActorKind.Enemy,
                SourceFilter.You   => ActorKind.You,
                SourceFilter.Party => ActorKind.Party,
                _                  => ActorKind.Other,
            };
            if (e.SourceKind != want) return false;
        }
        if (!RoleMatches(c.SourceRole, e.SourceId)) return false;
        if (!RoleMatches(c.TargetRole, e.TargetId)) return false;

        uint me = Plugin.PlayerState.EntityId;
        if (c.OnlyOnSelf)
        {
            bool self = on switch
            {
                FollowUpOn.Tether => e.SourceId == me || e.TargetId == me,
                FollowUpOn.Death  => e.SourceId == me,
                _                 => e.TargetId == me,
            };
            if (!self) return false;
        }

        if (on is FollowUpOn.Headmarker or FollowUpOn.Tether)
            return c.DataId == 0 || e.DataId == c.DataId;

        if (on == FollowUpOn.StatusGain)
        {
            if (StatusEventMatches(c, e)) return true;
            uint actor = c.OnlyOnSelf ? me : e.TargetId;
            return ActorHasStatus(actor, c);
        }

        if (c.MatchById && c.DataId != 0) return e.DataId == c.DataId;

        return string.IsNullOrWhiteSpace(c.Pattern) ||
               e.Name.Contains(c.Pattern, StringComparison.OrdinalIgnoreCase);
    }

    private static bool StatusEventMatches(FollowCond c, LogEvent e)
    {
        if (c.MatchById && c.DataId != 0 && e.DataId == c.DataId) return true;
        if (!string.IsNullOrWhiteSpace(c.Pattern))
            return e.Name.Contains(c.Pattern, StringComparison.OrdinalIgnoreCase);
        return !(c.MatchById && c.DataId != 0);
    }

    private bool ActorHasStatus(uint actorId, FollowCond c)
    {
        if (actorId == 0) return false;
        if (Plugin.ObjectTable.SearchById(actorId) is not IBattleChara bc) return false;

        bool byId   = c.MatchById && c.DataId != 0;
        bool byName = !string.IsNullOrWhiteSpace(c.Pattern);
        foreach (var s in bc.StatusList)
        {
            if (s.StatusId == 0) continue;
            if (byId && s.StatusId == c.DataId) return true;
            if (byName)
            {
                var name = Plugin.DataManager.GetExcelSheet<Lumina.Excel.Sheets.Status>()
                    .GetRowOrDefault(s.StatusId)?.Name.ExtractText();
                if (name != null && name.Contains(c.Pattern, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            if (!byId && !byName) return true;
        }
        return false;
    }

    private bool RegexMatch(string pattern, string input)
    {
        try
        {
            if (!_regexCache.TryGetValue(pattern, out var rx))
            {
                rx = new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.Compiled);
                _regexCache[pattern] = rx;
            }
            return rx.IsMatch(input);
        }
        catch { return false; }
    }

    public void Tick()
    {
        var now = DateTime.Now;

        for (int i = _pending.Count - 1; i >= 0; i--)
        {
            if (_pending[i].when > now) continue;
            var (_, t, e, key) = _pending[i];
            _pending.RemoveAt(i);
            Fire(t, e, key);
        }

        for (int i = _pendingFollow.Count - 1; i >= 0; i--)
        {
            if (_pendingFollow[i].when > now) continue;
            var (_, s, ctx, key) = _pendingFollow[i];
            _pendingFollow.RemoveAt(i);
            FireStep(s, ctx, key);
        }

        for (int i = _armedFollow.Count - 1; i >= 0; i--)
            if (_armedFollow[i].Expiry <= now) _armedFollow.RemoveAt(i);

        for (int i = _clearWatch.Count - 1; i >= 0; i--)
            if (_clearWatch[i].expiry <= now) _clearWatch.RemoveAt(i);

        foreach (var key in _live.Keys) PruneOwner(key);
    }

    private bool Matches(QuickDrawDef t, LogEvent e)
    {
        if (!t.AnyZone && t.Zones.Count > 0 &&
            !t.Zones.Contains(Plugin.ClientState.TerritoryType))
            return false;

        bool kindOk = t.On switch
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

        if (t.On == TriggerMatch.Chat)
        {
            if (string.IsNullOrEmpty(t.Pattern)) return true;
            if (t.UseRegex) return RegexMatch(t.Pattern, e.Name);
            return e.Name.Contains(t.Pattern, StringComparison.OrdinalIgnoreCase);
        }

        if (t.Source != SourceFilter.Anyone)
        {
            var want = t.Source switch
            {
                SourceFilter.Enemy => ActorKind.Enemy,
                SourceFilter.You   => ActorKind.You,
                SourceFilter.Party => ActorKind.Party,
                _                  => ActorKind.Other,
            };
            if (e.SourceKind != want) return false;
        }

        if (t.OnlyOnSelf)
        {
            uint me = Plugin.PlayerState.EntityId;
            if (e.Kind == LogKind.Tether)
            {
                if (e.SourceId != me && e.TargetId != me) return false;
            }
            else if (e.IsStatus || e.Kind == LogKind.Headmarker)
            {
                if (e.TargetId != me) return false;
            }
        }

        if (!RoleMatches(t.SourceRole, e.SourceId)) return false;
        if (!RoleMatches(t.TargetRole, e.TargetId)) return false;
        if (!NameContains(t.SourceName, e.SourceName)) return false;
        if (!NameContains(t.TargetName, e.TargetName)) return false;
        if (!NumMatches(t, e)) return false;
        if (!VarMatches(t, e)) return false;

        if (t.MatchById) return e.DataId == t.DataId;

        if (string.IsNullOrEmpty(t.Pattern)) return true;

        if (t.UseRegex) return RegexMatch(t.Pattern, e.Name);

        return e.Name.Contains(t.Pattern, StringComparison.OrdinalIgnoreCase);
    }

    private static readonly Regex VarTokenRx = new(@"\{\$(\w+)\}", RegexOptions.Compiled);

    private string Substitute(string text, LogEvent e)
    {
        if (string.IsNullOrEmpty(text)) return text;

        if (text.Contains("{$", StringComparison.Ordinal))
            text = VarTokenRx.Replace(text, m =>
                _vars.TryGetValue(m.Groups[1].Value, out var v) ? v : "");

        text = text
            .Replace("{name}", e.Name, StringComparison.OrdinalIgnoreCase)
            .Replace("{source}", e.SourceName, StringComparison.OrdinalIgnoreCase)
            .Replace("{target}", e.TargetName, StringComparison.OrdinalIgnoreCase);

        return text;
    }

    private static bool NameContains(string want, string actual)
        => string.IsNullOrWhiteSpace(want) ||
           (!string.IsNullOrEmpty(actual) && actual.Contains(want, StringComparison.OrdinalIgnoreCase));

    private static bool RoleMatches(RoleFilter want, uint actorId)
    {
        if (want == RoleFilter.Any) return true;
        if (actorId == 0) return false;
        if (Plugin.ObjectTable.SearchById(actorId) is not IBattleChara bc || !bc.ClassJob.IsValid)
            return false;
        return RoleClass(bc.ClassJob.Value.Role) == want;
    }

    private static RoleFilter RoleClass(byte role) => role switch
    {
        1      => RoleFilter.Tank,
        4      => RoleFilter.Healer,
        2 or 3 => RoleFilter.Dps,
        _      => RoleFilter.Any,
    };

    private bool NumMatches(QuickDrawDef t, LogEvent e)
    {
        if (t.NumConds.Count == 0) return true;
        foreach (var c in t.NumConds)
        {
            float v = ReadField(c.Field, e);
            if ((c.Field is NumField.SourceHpPct or NumField.TargetHpPct) && v < 0f) return false;
            if (!Compare(v, c.Op, c.Value)) return false;
        }
        return true;
    }

    private static float ReadField(NumField f, LogEvent e) => f switch
    {
        NumField.StackCount  => e.Count,
        NumField.Value       => e.Value,
        NumField.Param1      => e.Param1,
        NumField.Param2      => e.Param2,
        NumField.Param3      => e.Param3,
        NumField.Param4      => e.Param4,
        NumField.SourceHpPct => HpPct(e.SourceId),
        NumField.TargetHpPct => HpPct(e.TargetId),
        _                    => 0f,
    };

    private static float HpPct(uint actorId)
    {
        if (actorId == 0) return -1f;
        if (Plugin.ObjectTable.SearchById(actorId) is IBattleChara bc && bc.MaxHp > 0)
            return (float)bc.CurrentHp / bc.MaxHp * 100f;
        return -1f;
    }

    private static bool Compare(float a, NumOp op, float b) => op switch
    {
        NumOp.Eq => Math.Abs(a - b) < 0.0001f,
        NumOp.Ne => Math.Abs(a - b) >= 0.0001f,
        NumOp.Lt => a < b,
        NumOp.Le => a <= b,
        NumOp.Gt => a > b,
        NumOp.Ge => a >= b,
        _        => true,
    };

    private bool VarMatches(QuickDrawDef t, LogEvent e)
    {
        if (t.VarConds.Count == 0) return true;
        foreach (var c in t.VarConds)
        {
            if (string.IsNullOrWhiteSpace(c.Name)) continue;
            _vars.TryGetValue(c.Name, out var have);
            have ??= "";
            var want = Substitute(c.Value, e);
            bool ok;
            if (c.Numeric)
            {
                float hv = float.TryParse(have, NumberStyles.Float, CultureInfo.InvariantCulture, out var h) ? h : 0f;
                float wv = float.TryParse(want, NumberStyles.Float, CultureInfo.InvariantCulture, out var w) ? w : 0f;
                ok = Compare(hv, c.Op, wv);
            }
            else
            {
                int cmp = string.Compare(have, want, StringComparison.OrdinalIgnoreCase);
                ok = c.Op switch
                {
                    NumOp.Eq => cmp == 0,
                    NumOp.Ne => cmp != 0,
                    NumOp.Lt => cmp < 0,
                    NumOp.Le => cmp <= 0,
                    NumOp.Gt => cmp > 0,
                    NumOp.Ge => cmp >= 0,
                    _        => true,
                };
            }
            if (!ok) return false;
        }
        return true;
    }

    private void ApplyVars(QuickDrawDef t, LogEvent e)
    {
        if (t.SetVars.Count == 0) return;
        foreach (var a in t.SetVars)
        {
            if (string.IsNullOrWhiteSpace(a.Name)) continue;
            var val = Substitute(a.Value, e);
            if (a.Op == VarOp.Increment)
            {
                _vars.TryGetValue(a.Name, out var cur);
                float c   = float.TryParse(cur, NumberStyles.Float, CultureInfo.InvariantCulture, out var cc) ? cc : 0f;
                float add = float.TryParse(val, NumberStyles.Float, CultureInfo.InvariantCulture, out var av) ? av : 1f;
                _vars[a.Name] = (c + add).ToString(CultureInfo.InvariantCulture);
            }
            else _vars[a.Name] = val;
        }
    }

    private static Concurrency ModeOf(QuickDrawDef t)
        => t.NoReentry && t.Concurrency == Concurrency.Stack ? Concurrency.Wait : t.Concurrency;

    private void ArmClear(QuickDrawDef t, string key, uint subject)
    {
        if (!t.ClearOn.Enabled) return;
        _clearWatch.Add((DateTime.Now.AddSeconds(Math.Max(0.5f, t.ClearOn.Seconds)), t, key, subject));
    }

    private void ProcessClearWatch(LogEvent e)
    {
        if (_clearWatch.Count == 0) return;
        var now = DateTime.Now;
        for (int i = _clearWatch.Count - 1; i >= 0; i--)
        {
            var (expiry, t, key, subject) = _clearWatch[i];
            if (now > expiry) { _clearWatch.RemoveAt(i); continue; }
            if (!ClearMatches(t.ClearOn, e)) continue;
            // For a per-actor instance, only wipe it when the clearing event is
            // about that same actor.
            if (subject != 0 && EventSubject(e) != subject) continue;
            ClearOwner(key);
            _clearWatch.RemoveAt(i);
        }
    }

    private static bool ClearMatches(ClearRule r, LogEvent e)
    {
        if (!KindMatches(r.On, e)) return false;
        if (r.OnlyOnSelf)
        {
            uint me = Plugin.PlayerState.EntityId;
            bool self = r.On switch
            {
                FollowUpOn.Tether => e.SourceId == me || e.TargetId == me,
                FollowUpOn.Death  => e.SourceId == me,
                _                 => e.TargetId == me,
            };
            if (!self) return false;
        }
        if (r.MatchById && r.DataId != 0) return e.DataId == r.DataId;
        return string.IsNullOrWhiteSpace(r.Pattern) ||
               e.Name.Contains(r.Pattern, StringComparison.OrdinalIgnoreCase);
    }
}
