using System;
using System.Collections.Generic;
using System.Numerics;

namespace YapYapDraw.QuickDraws;

public enum TriggerMatch : byte
{
    Any,
    Cast,        // a cast bar starts (the telegraph moment)
    StatusGain,
    StatusLose,
    Death,
    Headmarker,
    Tether,
    Chat,
    CastEnd,     // the cast resolves / an instant goes off (the snapshot moment)
}

public enum SourceFilter : byte
{
    Anyone,
    Enemy,
    You,
    Party,
}

public enum RoleFilter : byte
{
    Any,
    Tank,
    Healer,
    Dps,
}

public enum NumField : byte
{
    StackCount,
    Value,
    SourceHpPct,
    TargetHpPct,
    Param1,
    Param2,
    Param3,
    Param4,
    DistSourceToTarget,
    DistMeToSource,
    DistMeToTarget,
}

public enum StatusGateWho : byte { Self, Source, Target }

public sealed class StatusGate
{
    public StatusGateWho Who      { get; set; } = StatusGateWho.Self;
    public bool          Have     { get; set; } = true;
    public uint          StatusId { get; set; }
    public string        Name     { get; set; } = "";

    public StatusGate Clone() => (StatusGate)MemberwiseClone();
}

public enum NumOp : byte { Eq, Ne, Lt, Le, Gt, Ge }

public sealed class NumCond
{
    public NumField Field { get; set; } = NumField.StackCount;
    public NumOp    Op    { get; set; } = NumOp.Ge;
    public float    Value { get; set; }

    public NumCond Clone() => (NumCond)MemberwiseClone();
}

public enum VarOp : byte { Set, Increment }

public enum Concurrency : byte
{
    Wait,
    Replace,
    Stack,
}

public sealed class VarAction
{
    public string Name  { get; set; } = "";
    public VarOp  Op    { get; set; } = VarOp.Set;
    public string Value { get; set; } = "1";

    public VarAction Clone() => (VarAction)MemberwiseClone();
}

public sealed class VarCond
{
    public string Name    { get; set; } = "";
    public NumOp  Op      { get; set; } = NumOp.Eq;
    public string Value   { get; set; } = "";
    public bool   Numeric { get; set; }

    public VarCond Clone() => (VarCond)MemberwiseClone();
}

// Removes this trigger's drawn shape early when a matching event arrives within
// the window (e.g. clear the telegraph the moment the cast goes off).
public sealed class ClearRule
{
    public bool       Enabled    { get; set; }
    public FollowUpOn On         { get; set; } = FollowUpOn.Cast;
    public float      Seconds    { get; set; } = 12f;
    public string     Pattern    { get; set; } = "";
    public uint       DataId     { get; set; }
    public bool       MatchById  { get; set; }
    public bool       OnlyOnSelf { get; set; }

    public ClearRule Clone() => (ClearRule)MemberwiseClone();
}

public enum FollowUpOn : byte
{
    Timer,
    Cast,
    StatusGain,
    StatusLose,
    Headmarker,
    Tether,
    Death,
    Chat,
    CastEnd,
}

public sealed class FollowCond
{
    public string Pattern    { get; set; } = "";
    public uint   DataId     { get; set; }
    public bool   MatchById  { get; set; }
    public bool   OnlyOnSelf { get; set; } = true;
    public bool   UseRegex   { get; set; }

    public SourceFilter Source     { get; set; } = SourceFilter.Anyone;
    public RoleFilter   SourceRole { get; set; } = RoleFilter.Any;
    public RoleFilter   TargetRole { get; set; } = RoleFilter.Any;

    public FollowCond Clone() => (FollowCond)MemberwiseClone();
}

public enum QuickShape : byte { Circle, Donut, Fan, Rectangle, Line, Tower, Knockback, Laser, Text, Arrow, ChevronPath }

// Where the shape is anchored when it fires.
public enum DrawAnchor : byte
{
    Source,        // the caster / status applier / tether source
    Target,        // who it landed on
    Self,          // the local player
    FixedPosition, // a typed-in arena coordinate
    EventPosition, // the position carried by the event (cast/effect origin)
    TetheredToMe,  // the live actor currently tethered to the local player
    WaymarkA,      // field markers, in game order A-D then 1-4
    WaymarkB,
    WaymarkC,
    WaymarkD,
    Waymark1,
    Waymark2,
    Waymark3,
    Waymark4,
    ArenaCenter,   // the arena centre (100, 100 convention)
    LinkedShape,
    NearbyActorById,
}

// The far end of a Line, or who a shape re-targets to.
public enum LinkTarget : byte
{
    EventTarget,          // who the event landed on
    EventSource,          // who cast / applied it
    MyTarget,             // the local player's current target
    NearestPlayer,        // closest other player to the anchor
    NearestEnemy,         // closest enemy to the anchor
    PlayerWithSameStatus, // another party member carrying the same debuff
    FixedSpot,            // a typed-in / clicked arena coordinate
    TetheredToMe,         // the live actor currently tethered to the local player
    WaymarkA,
    WaymarkB,
    WaymarkC,
    WaymarkD,
    Waymark1,
    Waymark2,
    Waymark3,
    Waymark4,
    ArenaCenter,
    LinkedShape,
}

// An optional stock-game look that overrides the smooth telegraph. Plain keeps
// the normal shape; the rest swap in a game omen for knockbacks / lasers / a
// pasted path (e.g. one copied out of the VFX log).
public enum VfxStyle : byte
{
    Plain,
    Knockback,
    Laser,
    Theater,
    Triangle,
    Custom,
}

// The drawn-shape parameters. Lives on both the trigger and its follow-up steps.
public sealed class DrawSpec
{
    public string     Id            { get; set; } = Guid.NewGuid().ToString("N");
    public string     AnchorShapeId { get; set; } = "";
    public string     LinkShapeId   { get; set; } = "";
    public uint       AnchorActorBaseId { get; set; }

    public QuickShape Shape       { get; set; } = QuickShape.Circle;
    public Vector4    Color       { get; set; } = new(1f, 0.55f, 0.10f, 1f);

    public float Radius      { get; set; } = 6f;    // circle radius / fan length / donut outer
    public float InnerRadius { get; set; } = 8f;    // donut inner
    public float HalfWidth   { get; set; } = 4f;    // rectangle / line half-width / arrowhead size
    public float Length      { get; set; } = 20f;   // rectangle length (front) / arrow length
    public int   FanAngle    { get; set; } = 90;    // fan full angle, degrees
    public float Rotation    { get; set; }          // facing, degrees (world, or offset-from-facing)

    public float ChevronSpacing { get; set; } = 2f;  // chevron path: a V every N yalms
    public float LineThickness  { get; set; } = 4f;  // arrow / chevron stroke width, pixels

    // When on (and stuck to an actor) the shape spins with that actor's heading,
    // and Rotation becomes an offset from "straight ahead".
    public bool OrientToFacing { get; set; }

    // Local nudge from the anchor, in the actor's own frame (or world if fixed).
    public float OffsetForward { get; set; }
    public float OffsetSide    { get; set; }

    public DrawAnchor Anchor        { get; set; } = DrawAnchor.Source;
    public bool       AttachToActor { get; set; } = true;
    public Vector3    FixedPosition { get; set; } = new(100f, 0f, 100f);

    // For Line shapes: where the far end attaches.
    public LinkTarget Link        { get; set; } = LinkTarget.EventTarget;
    public Vector3    LinkPosition { get; set; } = new(100f, 0f, 100f);

    // Rectangle only: stretch from the anchor to the far end (Link) like a wide line,
    // instead of a fixed length in a set facing.
    public bool SpanToTarget { get; set; }

    // For TetheredToMe anchor/link: 0 = any tether, else only match this tether id.
    public uint TetherFilterId { get; set; }

    public VfxStyle Style     { get; set; } = VfxStyle.Plain;
    public string   CustomVfx { get; set; } = "";

    public float Duration         { get; set; } = 5f; // seconds
    public bool  UseEventDuration { get; set; }        // match cast / debuff time

    // Spawn the shape this many times, each rotated a further RepeatStep degrees
    // around the anchor (also sweeps the offset), e.g. 8 fans around the boss.
    public int   Repeat     { get; set; } = 1;
    public float RepeatStep { get; set; } = 45f;

    // Hold off this many seconds before this shape appears (lets a multi-shape
    // draw play out as a timed sequence).
    public float StartDelay { get; set; }

    // Optional text drawn at the shape's centre (callout word, number, letter).
    public string  Label       { get; set; } = "";
    public Vector4 LabelColor  { get; set; } = new(1f, 1f, 1f, 1f);
    public float   LabelSize   { get; set; } = 1f;   // text scale multiplier
    public float   LabelHeight { get; set; } = 2f;   // yalms up off the floor

    public void NormalizeLegacy()
    {
        if (Style == VfxStyle.Plain) return;
        switch (Style)
        {
            case VfxStyle.Knockback:
                Shape = QuickShape.Knockback;
                break;
            case VfxStyle.Laser:
                Shape = QuickShape.Laser;
                break;
            case VfxStyle.Theater:
                Shape = QuickShape.Rectangle;
                break;
            case VfxStyle.Triangle:
                Shape = QuickShape.Fan;
                if (FanAngle < 30) FanAngle = 60;
                break;
        }
        Style = VfxStyle.Plain;
    }

    public void EnsureId()
    {
        if (string.IsNullOrEmpty(Id)) Id = Guid.NewGuid().ToString("N");
    }

    public DrawSpec Clone() => (DrawSpec)MemberwiseClone();
}

public sealed class FollowUpStep
{
    public string     Id  { get; set; } = Guid.NewGuid().ToString("N");
    public FollowUpOn On  { get; set; } = FollowUpOn.Timer;

    public float  Seconds    { get; set; } = 9f;

    public string Pattern    { get; set; } = "";
    public uint   DataId     { get; set; }
    public bool   OnlyOnSelf { get; set; } = true;

    public bool                  RequireAll { get; set; } = true;
    public List<FollowCond>      Conditions { get; set; } = new();

    public bool     DrawEnabled { get; set; } = true;
    public DrawSpec Draw        { get; set; } = new();

    // Additional shapes drawn together with Draw when this step fires.
    public List<DrawSpec> ExtraShapes { get; set; } = new();

    public bool IsConditional => On != FollowUpOn.Timer;

    public void EnsureConditions()
    {
        if (On == FollowUpOn.Timer || Conditions.Count > 0) return;
        Conditions.Add(new FollowCond
        {
            Pattern    = Pattern,
            DataId     = DataId,
            MatchById  = DataId != 0 && (On is FollowUpOn.Headmarker or FollowUpOn.Tether),
            OnlyOnSelf = OnlyOnSelf,
        });
    }

    public FollowUpStep Clone()
    {
        var c = (FollowUpStep)MemberwiseClone();
        c.Conditions = Conditions.ConvertAll(x => x.Clone());
        c.Draw       = Draw.Clone();
        c.ExtraShapes = ExtraShapes.ConvertAll(x => x.Clone());
        return c;
    }
}

public sealed class QuickDrawDef
{
    public string Id      { get; set; } = Guid.NewGuid().ToString("N");
    public bool   Enabled { get; set; } = true;
    public string Name    { get; set; } = "New quick draw";
    public string Group   { get; set; } = "";

    public TriggerMatch On         { get; set; } = TriggerMatch.Cast;
    public string       Pattern    { get; set; } = "";
    public bool         UseRegex   { get; set; }
    public SourceFilter Source     { get; set; } = SourceFilter.Anyone;
    public bool         OnlyOnSelf { get; set; }

    public bool MatchById { get; set; }
    public uint DataId    { get; set; }
    public uint IconId    { get; set; }

    public RoleFilter SourceRole { get; set; } = RoleFilter.Any;
    public RoleFilter TargetRole { get; set; } = RoleFilter.Any;
    public string     SourceName { get; set; } = "";
    public string     TargetName { get; set; } = "";

    public List<NumCond>    NumConds    { get; set; } = new();
    public List<VarCond>    VarConds    { get; set; } = new();
    public List<VarAction>  SetVars     { get; set; } = new();
    public List<StatusGate> StatusGates { get; set; } = new();

    public float       Cooldown    { get; set; }
    public bool        NoReentry   { get; set; }
    public Concurrency Concurrency { get; set; } = Concurrency.Replace;

    public ClearRule ClearOn { get; set; } = new();

    public bool       AnyZone { get; set; } = true;
    public List<uint> Zones   { get; set; } = new();

    public float DelaySeconds { get; set; }

    // The shape this fires.
    public bool     DrawEnabled { get; set; } = true;
    public DrawSpec Draw        { get; set; } = new();

    // Additional shapes drawn together with Draw on the same trigger.
    public List<DrawSpec> ExtraShapes { get; set; } = new();

    public List<FollowUpStep> FollowUps { get; set; } = new();

    public QuickDrawDef Clone()
    {
        var c = (QuickDrawDef)MemberwiseClone();
        c.FollowUps = FollowUps.ConvertAll(s => s.Clone());
        c.Zones     = new List<uint>(Zones);
        c.NumConds     = NumConds.ConvertAll(x => x.Clone());
        c.VarConds     = VarConds.ConvertAll(x => x.Clone());
        c.SetVars      = SetVars.ConvertAll(x => x.Clone());
        c.StatusGates  = StatusGates.ConvertAll(x => x.Clone());
        c.ClearOn   = ClearOn.Clone();
        c.Draw      = Draw.Clone();
        c.ExtraShapes = ExtraShapes.ConvertAll(x => x.Clone());
        return c;
    }
}
