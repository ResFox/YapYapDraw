using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Text.RegularExpressions;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Plugin.Services;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Enum;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.Util;
using YapYapDraw.Engine.Vfx;
using YapYapDraw.Logging;

namespace YapYapDraw.QuickDraws;

// Event -> match -> draw floor shape.
public sealed class QuickDrawEngine
{
    private const double SuppressSeconds = 2.5;

    private readonly Configuration    _config;
    private readonly IPluginLog       _log;
    private readonly CombatLogCapture _capture;
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
        public string    ShapeId = "";
    }
    private readonly Dictionary<string, List<Tracked>> _live = new();

    private sealed class ShapeAnchor
    {
        public Func<Vector3?> Pos = () => null;
        public Vector3        Last;
        public DateTime       Expiry;
        public IGameObject?   Owner;
    }
    private readonly Dictionary<string, ShapeAnchor> _shapeAnchors = new();

    // Shapes waiting on a per-shape start delay before they appear.
    private readonly List<(DateTime when, string ownerId, DrawSpec d, LogEvent e)> _pendingShape = new();

    private sealed class LiveLabel
    {
        public string         OwnerId = "";
        public Func<Vector3?> World   = () => null;
        public string         Text    = "";
        public Vector4        Color;
        public float          Size    = 1f;
        public DateTime       Expiry;
        public Vector2        Screen;
        public bool           HasScreen;
        public bool           FollowsActor;
        public Vector3        Anchor;
        public bool           AnchorInit;
        public BindKind       Bind;
        public uint           BindSrc;
        public uint           BindId;

        public Vector3 SmoothAnchor(Vector3 raw)
        {
            if (!AnchorInit)
            {
                Anchor = raw;
                AnchorInit = true;
                return raw;
            }

            Anchor = new Vector3(raw.X, Anchor.Y + (raw.Y - Anchor.Y) * 0.1f, raw.Z);
            return Anchor;
        }
    }
    private readonly List<LiveLabel> _labels = new();

    private sealed class LiveArrow
    {
        public string         OwnerId = "";
        public bool           Chevron;
        public Func<Vector3?> Origin = () => null;
        public Func<Vector3?> Target = () => null;
        public bool           HasTarget;
        public uint           HeadingId;
        public bool           Orient;
        public float          Rotation;   // radians
        public float          Length;
        public float          Spacing;
        public float          Thickness;
        public float          HeadSize;
        public Vector4        Color;
        public DateTime       Expiry;
        public BindKind       Bind;
        public uint           BindSrc;
        public uint           BindId;
    }
    private readonly List<LiveArrow> _arrows = new();

    public readonly record struct ArrowGeo(
        Vector3 Origin, float Angle, float Length, float Spacing,
        float Thickness, float HeadSize, Vector4 Color, bool Chevron);

    public IEnumerable<ArrowGeo> ActiveArrows()
    {
        var now = DateTime.Now;
        foreach (var a in _arrows)
        {
            if (a.Expiry <= now) continue;
            var o = a.Origin();
            if (o == null) continue;
            var origin = o.Value;

            float angle  = a.Rotation;
            float length = a.Length;

            if (a.HasTarget)
            {
                var t = a.Target?.Invoke();
                if (t == null) continue;
                float dx = t.Value.X - origin.X;
                float dz = t.Value.Z - origin.Z;
                float flat = MathF.Sqrt(dx * dx + dz * dz);
                if (flat >= 0.1f)
                {
                    angle  = MathF.Atan2(dx, dz) + a.Rotation;
                    length = flat;
                }
                else
                {
                    angle  = a.Rotation;
                    length = a.Length;
                }
            }
            else if (a.Orient && a.HeadingId != 0)
            {
                var ho = Plugin.ObjectTable.SearchById(a.HeadingId);
                angle = (ho?.Rotation ?? 0f) + a.Rotation;
            }

            yield return new ArrowGeo(origin, angle, MathF.Max(0.5f, length),
                MathF.Max(0.5f, a.Spacing), a.Thickness, a.HeadSize, a.Color, a.Chevron);
        }
    }

    public void RefreshLabelScreens()
    {
        var now = DateTime.Now;
        foreach (var l in _labels)
        {
            l.HasScreen = false;
            if (l.Expiry <= now) continue;
            var w = l.World();
            if (w == null) continue;
            var world = l.FollowsActor ? l.SmoothAnchor(w.Value) : w.Value;
            if (!PositionHelper.StableWorldToScreen(world, out var screen)) continue;
            l.Screen = screen;
            l.HasScreen = true;
        }
    }

    public IEnumerable<(Vector2 Screen, string Text, Vector4 Color, float Size)> ActiveLabelScreens()
    {
        var now = DateTime.Now;
        foreach (var l in _labels)
        {
            if (l.Expiry <= now || !l.HasScreen) continue;
            yield return (l.Screen, l.Text, l.Color, l.Size);
        }
    }

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

    public readonly record struct FireMark(DateTime When, string Draw, string Trigger);
    private readonly List<FireMark> _recentFires = new();
    public IReadOnlyList<FireMark> RecentFires => _recentFires;

    public QuickDrawEngine(Configuration config, IPluginLog log, CombatLogCapture capture)
    {
        _config  = config;
        _log     = log;
        _capture = capture;
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

                _recentFires.Add(new FireMark(DateTime.Now, t.Name, string.IsNullOrEmpty(e.Name) ? e.SourceName : e.Name));
                if (_recentFires.Count > 40) _recentFires.RemoveRange(0, _recentFires.Count - 40);

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

    // Reusable draw entry points for callers outside the QuickDraw matcher (strat
    // engine, IPC). They build a DrawSpec and let the engine own the live shape.
    public void SpawnExternal(string ownerId, DrawSpec d, LogEvent e, bool previewSelf = false)
        => SpawnShape(ownerId, d, e, previewSelf);

    public void ClearExternal(string ownerId) => ClearOwner(ownerId);

    private void Fire(QuickDrawDef t, LogEvent e, string key)
    {
        EnsureIds(t);
        if (ModeOf(t) == Concurrency.Replace) ClearOwner(key);

        if (t.DrawEnabled)
            SpawnSpec(key, t.Draw, e);
        foreach (var ex in t.ExtraShapes)
            SpawnSpec(key, ex, e);

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
            SpawnSpec(key, s.Draw, ctx);
        foreach (var ex in s.ExtraShapes)
            SpawnSpec(key, ex, ctx);
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
        EnsureIds(t);
        var sample = new LogEvent { Name = string.IsNullOrEmpty(t.Pattern) ? "Sample" : t.Pattern };
        ClearOwner(t.Id);
        if (t.DrawEnabled)
            SpawnShape(t.Id, t.Draw, sample, previewSelf: true);
        foreach (var ex in t.ExtraShapes)
            SpawnShape(t.Id, ex, sample, previewSelf: true);
    }

    public void PreviewShape(QuickDrawDef t, DrawSpec d)
    {
        EnsureIds(t);
        var sample = new LogEvent { Name = "Sample" };
        ClearOwner("preview_shape");
        foreach (var dep in DependencyShapes(t, d))
            SpawnShape("preview_shape", dep, sample, previewSelf: true);
        SpawnShape("preview_shape", d, sample, previewSelf: true);
    }

    public static void EnsureIds(QuickDrawDef t)
    {
        t.Draw.EnsureId();
        foreach (var ex in t.ExtraShapes) ex.EnsureId();
        foreach (var s in t.FollowUps)
        {
            s.Draw.EnsureId();
            foreach (var ex in s.ExtraShapes) ex.EnsureId();
        }
        EnsureLinkIds(t);
    }

    private static void EnsureLinkIds(QuickDrawDef t)
    {
        foreach (var d in EnumerateDraws(t))
        {
            if (d.Anchor == DrawAnchor.LinkedShape && string.IsNullOrEmpty(d.AnchorShapeId))
            {
                var pick = FirstLinkableShape(t, d.Id);
                if (pick != null) d.AnchorShapeId = pick;
            }
            if (d.Link == LinkTarget.LinkedShape && string.IsNullOrEmpty(d.LinkShapeId))
            {
                var pick = FirstLinkableShape(t, d.Id);
                if (pick != null) d.LinkShapeId = pick;
            }
        }
    }

    private static IEnumerable<DrawSpec> EnumerateDraws(QuickDrawDef t)
    {
        yield return t.Draw;
        foreach (var ex in t.ExtraShapes) yield return ex;
        foreach (var fu in t.FollowUps)
        {
            yield return fu.Draw;
            foreach (var ex in fu.ExtraShapes) yield return ex;
        }
    }

    private static string? FirstLinkableShape(QuickDrawDef t, string excludeId)
    {
        if (t.Draw.Id != excludeId) return t.Draw.Id;
        foreach (var ex in t.ExtraShapes)
            if (ex.Id != excludeId) return ex.Id;
        foreach (var fu in t.FollowUps)
        {
            if (fu.Draw.Id != excludeId) return fu.Draw.Id;
            foreach (var ex in fu.ExtraShapes)
                if (ex.Id != excludeId) return ex.Id;
        }
        return null;
    }

    private void SpawnShape(string ownerId, DrawSpec d, LogEvent e, bool previewSelf = false)
    {
        Vector3? pos = ResolvePosition(d, e, previewSelf, out IGameObject? attach);
        if (pos == null && attach == null && d.Anchor != DrawAnchor.LinkedShape) return;

        int repeat = Math.Max(1, d.Repeat);
        for (int i = 0; i < repeat; i++)
        {
            var ds = d;
            if (i > 0)
            {
                float rotOffset = i * d.RepeatStep;
                ds = d.Clone();
                ds.Rotation = d.Rotation + rotOffset;
                // Sweep the nudge around the anchor too, so offset shapes ring it.
                float rad = rotOffset * MathF.PI / 180f;
                float c = MathF.Cos(rad), s = MathF.Sin(rad);
                ds.OffsetForward = d.OffsetForward * c - d.OffsetSide * s;
                ds.OffsetSide    = d.OffsetForward * s + d.OffsetSide * c;
            }
            SpawnOne(ownerId, ds, e, pos, attach);
        }

        SpawnLabel(ownerId, d, e, pos, attach, previewSelf);

        // Text / Arrow / Path make no floor VFX, so register their spot here or
        // nothing else could connect to them.
        if (d.Shape is QuickShape.Text or QuickShape.Arrow or QuickShape.ChevronPath)
        {
            d.EnsureId();
            RegisterPointAnchor(d.Id, BuildAnchorPosFunc(d, pos, attach), ResolveEventLife(d, e));
        }
    }

    private Func<Vector3?> BuildAnchorPosFunc(DrawSpec d, Vector3? pos, IGameObject? attach)
    {
        bool glue = d.AttachToActor && attach != null && d.Anchor != DrawAnchor.LinkedShape;
        uint followId = glue ? attach!.EntityId : 0;
        Vector3 fixedPos = pos ?? (attach != null ? attach.Position : new Vector3(100f, 0f, 100f));
        if (followId != 0)
            return () => Plugin.ObjectTable.SearchById(followId)?.Position ?? fixedPos;
        if (d.Anchor == DrawAnchor.LinkedShape)
            return () => ResolveLinkedShapePos(d.AnchorShapeId) ?? fixedPos;
        return () => fixedPos;
    }

    private void RegisterPointAnchor(string id, Func<Vector3?> pos, float life)
    {
        if (string.IsNullOrEmpty(id)) return;
        _shapeAnchors[id] = new ShapeAnchor
        {
            Expiry = DateTime.Now.AddSeconds(life),
            Pos    = pos,
        };
    }

    private void SpawnOne(string ownerId, DrawSpec d, LogEvent e, Vector3? pos, IGameObject? attach)
    {
        if (d.Shape == QuickShape.Text) return;
        if (d.Shape is QuickShape.Arrow or QuickShape.ChevronPath)
        {
            SpawnArrow(ownerId, d, e, pos, attach);
            return;
        }

        d.EnsureId();
        float life = ResolveEventLife(d, e);
        float ms   = Math.Max(0.1f, life) * 1000f;
        bool  glue = d.AttachToActor && attach != null && d.Anchor != DrawAnchor.LinkedShape;
        var   linkOwner = d.Anchor == DrawAnchor.LinkedShape ? ResolveLinkedShapeOwner(d.AnchorShapeId) : null;
        bool  faceActor = d.OrientToFacing && (glue ? attach != null : linkOwner != null);

        var elem = new DrawElement
        {
            Position     = pos ?? (attach != null ? attach.RenderPosition() : new Vector3(100, 0, 100)),
            drawOnObject = glue,
            refColor       = d.Color,
            refTargetColor = d.Color,
            destroyTime  = ms,
            refOffsetZ = -d.OffsetForward,
            refOffsetX = -d.OffsetSide,
        };

        if (d.Anchor == DrawAnchor.LinkedShape)
        {
            elem.drawOnObject = false;
            elem.PositionCustomAction = () => ResolveLinkedShapePos(d.AnchorShapeId) ?? elem.Position;
            if (d.OrientToFacing && linkOwner != null)
            {
                elem.fixRotation = false;
                elem.RotationCustomAction = () =>
                {
                    var o = ResolveLinkedShapeOwner(d.AnchorShapeId);
                    return o != null ? o.Rotation.Radians() + d.Rotation.Degrees() : d.Rotation.Degrees();
                };
            }
        }

        IGameObject? owner = glue ? attach : null;

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
                // Stock game fan omens carry baked-in art that muddies a flat tint
                // (cyan in particular), so build a clean fan that takes the colour.
                elem.drawAvfx = "customFan";
                elem.refRadian = d.FanAngle.Degrees().Rad;
                elem.radiusX = d.Radius; elem.radiusZ = d.Radius;
                ApplyRotation(elem, d, faceActor);
                break;
            case QuickShape.Rectangle:
                if (d.SpanToTarget)
                {
                    SetupLine(elem, d, e, attach, glue);
                }
                else
                {
                    elem.drawAvfx = "customRect";
                    elem.radiusX = d.HalfWidth;
                    elem.radiusZ = d.Length;
                    ApplyRotation(elem, d, faceActor);
                }
                break;
            case QuickShape.Line:
                SetupLine(elem, d, e, attach, glue);
                break;
            case QuickShape.Tower:
                elem.drawAvfx = GroundOmen.SingleTowerSilent;
                elem.radiusX = d.Radius; elem.radiusZ = d.Radius; elem.radiusY = 1f;
                break;
            case QuickShape.Knockback:
                elem.drawAvfx = GroundOmen.KnockBackSilent;
                elem.radiusX = d.Radius; elem.radiusZ = d.Radius; elem.radiusY = 1f;
                ApplyRotation(elem, d, faceActor);
                break;
            case QuickShape.Laser:
                elem.drawAvfx = GroundOmen.ArrowRectSilent;
                elem.radiusX = d.HalfWidth;
                elem.radiusZ = d.Length;
                elem.radiusY = 1f;
                ApplyRotation(elem, d, faceActor);
                break;
        }

        var vfx = DrawManager.Draw(elem, glue ? owner : null);
        if (vfx == null) return;

        if (!_live.TryGetValue(ownerId, out var list)) { list = new(); _live[ownerId] = list; }

        var tracked = new Tracked { Vfx = vfx, Expiry = DateTime.Now.AddSeconds(life), ShapeId = d.Id };
        if (d.UseEventDuration)
        {
            if (e.Kind == LogKind.CastStart && e.SourceId != 0)
            { tracked.Bind = BindKind.Cast; tracked.BindSrc = e.SourceId; tracked.BindId = e.DataId; }
            else if (e.Kind == LogKind.StatusGain && e.TargetId != 0)
            { tracked.Bind = BindKind.Status; tracked.BindSrc = e.TargetId; tracked.BindId = e.DataId; }
        }
        list.Add(tracked);
        RegisterShapeAnchor(d.Id, vfx, life);
    }

    private void SpawnLabel(string ownerId, DrawSpec d, LogEvent e, Vector3? pos, IGameObject? attach, bool previewSelf)
    {
        if (string.IsNullOrWhiteSpace(d.Label)) return;

        float life = ResolveEventLife(d, e);
        bool  glue = d.AttachToActor && attach != null;
        uint  followId = glue ? attach!.EntityId : 0;
        Vector3 fixedPos = pos ?? (attach != null ? attach.Position : new Vector3(100, 0, 100));
        if (d.Anchor == DrawAnchor.LinkedShape)
        {
            followId = 0;
            fixedPos = ResolveLinkedShapePos(d.AnchorShapeId) ?? fixedPos;
        }
        var up  = new Vector3(0f, d.LabelHeight, 0f);
        var col = d.LabelColor.W <= 0.01f ? d.LabelColor with { W = 1f } : d.LabelColor;

        _labels.Add(new LiveLabel
        {
            OwnerId      = ownerId,
            FollowsActor = followId != 0,
            World        = followId != 0
                ? () =>
                {
                    var o = Plugin.ObjectTable.SearchById(followId);
                    return o == null ? null : o.Position + up;
                }
                : d.Anchor == DrawAnchor.LinkedShape
                    ? () => (ResolveLinkedShapePos(d.AnchorShapeId) ?? fixedPos) + up
                    : () => fixedPos + up,
            Text    = d.Label,
            Color   = col,
            Size    = MathF.Max(0.3f, d.LabelSize),
            Expiry  = DateTime.Now.AddSeconds(life),
        });

        var label = _labels[^1];
        if (d.UseEventDuration)
        {
            if (e.Kind == LogKind.CastStart && e.SourceId != 0)
            { label.Bind = BindKind.Cast; label.BindSrc = e.SourceId; label.BindId = e.DataId; }
            else if (e.Kind == LogKind.StatusGain && e.TargetId != 0)
            { label.Bind = BindKind.Status; label.BindSrc = e.TargetId; label.BindId = e.DataId; }
        }
    }

    private static bool IsPositionalLink(LinkTarget l) =>
        l == LinkTarget.FixedSpot || l == LinkTarget.ArenaCenter ||
        (l >= LinkTarget.WaymarkA && l <= LinkTarget.Waymark4);

    private void SpawnArrow(string ownerId, DrawSpec d, LogEvent e, Vector3? pos, IGameObject? attach)
    {
        d.EnsureId();
        float life = ResolveEventLife(d, e);
        bool  glue = d.AttachToActor && attach != null && d.Anchor != DrawAnchor.LinkedShape;
        uint  followId = glue ? attach!.EntityId : 0;
        Vector3 fixedPos = pos ?? (attach != null ? attach.Position : new Vector3(100, 0, 100));

        Func<Vector3?> originFn =
            followId != 0
                ? () => Plugin.ObjectTable.SearchById(followId)?.Position
                : d.Anchor == DrawAnchor.LinkedShape
                    ? () => ResolveLinkedShapePos(d.AnchorShapeId) ?? fixedPos
                    : () => fixedPos;

        var  farActor = ResolveLink(d, e, attach);
        uint farId    = farActor?.EntityId ?? 0;
        Func<Vector3?> targetFn;
        bool hasTarget = true;
        if (farId != 0)
            targetFn = () => Plugin.ObjectTable.SearchById(farId)?.Position;
        else if (d.Link == LinkTarget.LinkedShape)
            targetFn = () => ResolveLinkedShapePos(d.LinkShapeId);
        else if (IsPositionalLink(d.Link))
        {
            var fp = ResolveLinkPosition(d, e);
            targetFn = () => fp;
        }
        else
        {
            targetFn  = () => null;
            hasTarget = false;
        }

        var col = d.Color.W <= 0.01f ? d.Color with { W = 1f } : d.Color;

        var arrow = new LiveArrow
        {
            OwnerId   = ownerId,
            Chevron   = d.Shape == QuickShape.ChevronPath,
            Origin    = originFn,
            Target    = targetFn,
            HasTarget = hasTarget,
            HeadingId = followId,
            Orient    = d.OrientToFacing,
            Rotation  = d.Rotation * (MathF.PI / 180f),
            Length    = d.Length,
            Spacing   = d.ChevronSpacing,
            Thickness = MathF.Max(1f, d.LineThickness),
            HeadSize  = MathF.Max(0.5f, d.HalfWidth),
            Color     = col,
            Expiry    = DateTime.Now.AddSeconds(life),
        };
        if (d.UseEventDuration)
        {
            if (e.Kind == LogKind.CastStart && e.SourceId != 0)
            { arrow.Bind = BindKind.Cast; arrow.BindSrc = e.SourceId; arrow.BindId = e.DataId; }
            else if (e.Kind == LogKind.StatusGain && e.TargetId != 0)
            { arrow.Bind = BindKind.Status; arrow.BindSrc = e.TargetId; arrow.BindId = e.DataId; }
        }
        _arrows.Add(arrow);
    }

    // Spawn now, or hold for the shape's own start delay so a multi-shape draw can
    // play out as a timed sequence. Previews ignore the delay.
    private void SpawnSpec(string ownerId, DrawSpec d, LogEvent e, bool previewSelf = false)
    {
        if (!previewSelf && d.StartDelay > 0.01f)
            _pendingShape.Add((DateTime.Now.AddSeconds(d.StartDelay), ownerId, d, e));
        else
            SpawnShape(ownerId, d, e, previewSelf);
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
                UnregisterShapeAnchor(tr.ShapeId);
                try { tr.Vfx.Remove(); } catch { }
                list.RemoveAt(i);
            }
        _labels.RemoveAll(l => l.Bind == kind && l.BindSrc == src && l.BindId == id);
        _arrows.RemoveAll(a => a.Bind == kind && a.BindSrc == src && a.BindId == id);
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
        else if (d.Link == LinkTarget.LinkedShape)
            elem.TargetPositionCustomAction = () => ResolveLinkedShapePos(d.LinkShapeId) ?? elem.targetPosition;
        else
            elem.targetPosition = ResolveLinkPosition(d, e);
    }

    // The far end as a world point when it isn't a live actor (fixed spot, a
    // waymark, the arena centre, or the event's own position).
    private static Vector3 ResolveLinkPosition(DrawSpec d, LogEvent e)
    {
        switch (d.Link)
        {
            case LinkTarget.FixedSpot:
                return d.LinkPosition;
            case LinkTarget.ArenaCenter:
                return ArenaCenter;
            case >= LinkTarget.WaymarkA and <= LinkTarget.Waymark4:
                return Waymark((int)d.Link - (int)LinkTarget.WaymarkA) ?? d.LinkPosition;
            default:
                return new Vector3(e.X, 0f, e.Y);
        }
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
            case LinkTarget.TetheredToMe:
                return TetheredActor(d.TetherFilterId);
            case LinkTarget.LinkedShape:
                return null;
            case LinkTarget.FixedSpot:
            default:
                return null;
        }
    }

    // The live actor on the other end of a tether attached to the local player.
    // From is the tether owner (e.g. the clone), To is the target.
    private IGameObject? TetheredActor(uint tetherId)
    {
        uint me = Plugin.PlayerState.EntityId;
        if (me == 0) return null;
        foreach (var t in _capture.ActiveTethers)
        {
            if (tetherId != 0 && t.Id != tetherId) continue;
            if (t.To == me && t.From != 0) return Actor(t.From);
            if (t.From == me && t.To != 0) return Actor(t.To);
        }
        return null;
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

        // A preview has no real event actors, so everything but a real coordinate
        // (fixed spot, waymark, arena centre) falls back to the player — otherwise
        // "test" would always snap the shape onto you.
        if (previewSelf && d.Anchor != DrawAnchor.FixedPosition && d.Anchor < DrawAnchor.WaymarkA)
        {
            var me = LocalPlayer();
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
                attach = LocalPlayer();
                return attach?.Position;
            case DrawAnchor.EventPosition:
                return new Vector3(e.X, 0f, e.Y);
            case DrawAnchor.TetheredToMe:
                attach = TetheredActor(d.TetherFilterId);
                return attach?.Position;
            case DrawAnchor.ArenaCenter:
                return ArenaCenter;
            case >= DrawAnchor.WaymarkA and <= DrawAnchor.Waymark4:
                return Waymark((int)d.Anchor - (int)DrawAnchor.WaymarkA);
            case DrawAnchor.LinkedShape:
                return ResolveLinkedShapePos(d.AnchorShapeId);
            case DrawAnchor.NearbyActorById:
                attach = NearestByBaseId(d.AnchorActorBaseId, e);
                return attach?.RenderPosition();
            case DrawAnchor.FixedPosition:
            default:
                return d.FixedPosition;
        }
    }

    private static readonly Vector3 ArenaCenter = new(100f, 0f, 100f);

    // Field markers in game order A,B,C,D,1,2,3,4 (indices 0-7).
    private static unsafe Vector3? Waymark(int index)
    {
        var mc = FFXIVClientStructs.FFXIV.Client.Game.UI.MarkingController.Instance();
        if (mc == null) return null;
        int i = 0;
        foreach (ref var m in mc->FieldMarkers)
        {
            if (i == index)
                return m.Active ? new Vector3(m.X / 1000f, m.Y / 1000f, m.Z / 1000f) : null;
            i++;
        }
        return null;
    }

    private static IGameObject? Actor(uint id)
        => id == 0 ? null : Plugin.ObjectTable.SearchById(id);

    // The object-table local player is the reliable handle; the player-state id can
    // briefly fail to resolve and snap a Self-anchored shape onto arena centre.
    private static IGameObject? LocalPlayer()
        => Plugin.ObjectTable.LocalPlayer ?? Actor(Plugin.PlayerState.EntityId);

    private bool OwnerLive(string id)
    {
        PruneOwner(id);
        return _live.TryGetValue(id, out var list) && list.Count > 0;
    }

    private void ClearOwner(string id)
    {
        _pendingShape.RemoveAll(p => p.ownerId == id);
        _labels.RemoveAll(l => l.OwnerId == id);
        _arrows.RemoveAll(a => a.OwnerId == id);
        if (_live.TryGetValue(id, out var list))
        {
            foreach (var t in list)
            {
                UnregisterShapeAnchor(t.ShapeId);
                try { t.Vfx.Remove(); } catch { }
            }
            list.Clear();
        }
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

        for (int i = _pendingShape.Count - 1; i >= 0; i--)
        {
            if (_pendingShape[i].when > now) continue;
            var (_, oid, d, ev) = _pendingShape[i];
            _pendingShape.RemoveAt(i);
            SpawnShape(oid, d, ev);
        }

        for (int i = _armedFollow.Count - 1; i >= 0; i--)
            if (_armedFollow[i].Expiry <= now) _armedFollow.RemoveAt(i);

        for (int i = _clearWatch.Count - 1; i >= 0; i--)
            if (_clearWatch[i].expiry <= now) _clearWatch.RemoveAt(i);

        _labels.RemoveAll(l => l.Expiry <= now);
        _arrows.RemoveAll(a => a.Expiry <= now);

        foreach (var key in _shapeAnchors.Keys.ToList())
        {
            if (_shapeAnchors[key].Expiry <= now)
                _shapeAnchors.Remove(key);
        }

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
        if (!StatusMatches(t, e)) return false;

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
            if ((c.Field is NumField.SourceHpPct or NumField.TargetHpPct
                    or NumField.DistSourceToTarget or NumField.DistMeToSource or NumField.DistMeToTarget) && v < 0f) return false;
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
        NumField.DistSourceToTarget => ActorDist(e.SourceId, e.TargetId),
        NumField.DistMeToSource     => ActorDist(Plugin.PlayerState.EntityId, e.SourceId),
        NumField.DistMeToTarget     => ActorDist(Plugin.PlayerState.EntityId, e.TargetId),
        _                    => 0f,
    };

    private static float ActorDist(uint a, uint b)
    {
        if (a == 0 || b == 0) return -1f;
        var ao = Actor(a);
        var bo = Actor(b);
        if (ao == null || bo == null) return -1f;
        return Vector3.Distance(ao.Position, bo.Position);
    }

    private static float HpPct(uint actorId)
    {
        if (actorId == 0) return -1f;
        if (Plugin.ObjectTable.SearchById(actorId) is IBattleChara bc && bc.MaxHp > 0)
            return (float)bc.CurrentHp / bc.MaxHp * 100f;
        return -1f;
    }

    private bool StatusMatches(QuickDrawDef t, LogEvent e)
    {
        if (t.StatusGates.Count == 0) return true;
        foreach (var g in t.StatusGates)
        {
            uint actorId = g.Who switch
            {
                StatusGateWho.Self   => Plugin.PlayerState.EntityId,
                StatusGateWho.Source => e.SourceId,
                StatusGateWho.Target => e.TargetId,
                _                    => 0u,
            };
            if (!ActorStatusGate(actorId, g)) return false;
        }
        return true;
    }

    private static bool ActorStatusGate(uint actorId, StatusGate g)
    {
        bool has = ActorHasNamedStatus(actorId, g.StatusId, g.Name);
        return g.Have ? has : !has;
    }

    private static bool ActorHasNamedStatus(uint actorId, uint statusId, string name)
    {
        if (actorId == 0) return false;
        if (Plugin.ObjectTable.SearchById(actorId) is not IBattleChara bc) return false;
        bool byId   = statusId != 0;
        bool byName = !string.IsNullOrWhiteSpace(name);
        if (!byId && !byName) return false;
        foreach (var s in bc.StatusList)
        {
            if (s.StatusId == 0) continue;
            if (byId && s.StatusId == statusId) return true;
            if (byName)
            {
                var n = Plugin.DataManager.GetExcelSheet<Lumina.Excel.Sheets.Status>()
                    .GetRowOrDefault(s.StatusId)?.Name.ExtractText();
                if (n != null && n.Contains(name, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
        }
        return false;
    }

    private static IEnumerable<DrawSpec> DependencyShapes(QuickDrawDef t, DrawSpec d)
    {
        var seen = new HashSet<string>();
        if (d.Anchor == DrawAnchor.LinkedShape && !string.IsNullOrEmpty(d.AnchorShapeId))
        {
            var s = FindShape(t, d.AnchorShapeId);
            if (s != null && seen.Add(s.Id)) yield return s;
        }
        bool usesLink = d.Shape is QuickShape.Line or QuickShape.Arrow or QuickShape.ChevronPath
            || (d.Shape == QuickShape.Rectangle && d.SpanToTarget);
        if (usesLink && d.Link == LinkTarget.LinkedShape && !string.IsNullOrEmpty(d.LinkShapeId))
        {
            var s = FindShape(t, d.LinkShapeId);
            if (s != null && seen.Add(s.Id)) yield return s;
        }
    }

    private static DrawSpec? FindShape(QuickDrawDef t, string id)
    {
        if (t.Draw.Id == id) return t.Draw;
        foreach (var ex in t.ExtraShapes)
            if (ex.Id == id) return ex;
        foreach (var fu in t.FollowUps)
        {
            if (fu.Draw.Id == id) return fu.Draw;
            foreach (var ex in fu.ExtraShapes)
                if (ex.Id == id) return ex;
        }
        return null;
    }

    private Vector3? ResolveLinkedShapePos(string shapeId)
    {
        if (string.IsNullOrEmpty(shapeId)) return null;
        if (!_shapeAnchors.TryGetValue(shapeId, out var a)) return null;
        var p = a.Pos();
        if (p != null)
        {
            a.Last = p.Value;
            return p;
        }
        return a.Last != default ? a.Last : null;
    }

    private IGameObject? ResolveLinkedShapeOwner(string shapeId)
    {
        if (string.IsNullOrEmpty(shapeId)) return null;
        return _shapeAnchors.TryGetValue(shapeId, out var a) ? a.Owner : null;
    }

    private void RegisterShapeAnchor(string id, StaticVfx vfx, float life)
    {
        if (string.IsNullOrEmpty(id)) return;
        _shapeAnchors[id] = new ShapeAnchor
        {
            Expiry = DateTime.Now.AddSeconds(life),
            Owner  = vfx.Owner,
            Pos    = () =>
            {
                if (vfx.LastPosition != default)
                    return vfx.LastPosition;
                if (vfx.Position != default)
                    return vfx.Position;
                return null;
            },
        };
    }

    private void UnregisterShapeAnchor(string id)
    {
        if (!string.IsNullOrEmpty(id))
            _shapeAnchors.Remove(id);
    }

    private static IGameObject? NearestByBaseId(uint baseId, LogEvent e)
    {
        if (baseId == 0) return null;
        var origin = new Vector3(e.X, 0f, e.Y);
        if (origin.X == 0f && origin.Z == 0f)
        {
            var src = Actor(e.SourceId);
            origin = src?.Position ?? new Vector3(100f, 0f, 100f);
        }
        IGameObject? best = null;
        float bestSq = float.MaxValue;
        foreach (var o in Plugin.ObjectTable)
        {
            if (o.BaseId != baseId) continue;
            float dSq = Vector3.DistanceSquared(o.Position, origin);
            if (dSq < bestSq) { bestSq = dSq; best = o; }
        }
        return best;
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
