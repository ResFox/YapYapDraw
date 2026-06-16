using System;
using System.Collections.Generic;
using System.Numerics;
using YapYapDraw.QuickDraws;

namespace YapYapDraw.Strats;

public enum StratRole : byte { MT, OT, M1, M2, R1, R2, H1, H2 }

// How a branch is chosen at runtime. Manual is a one-tap pick; BossPosition reads
// the boss's compass side at the moment the slide fires; MyStatus checks whether
// the local player carries a given debuff/buff.
public enum BranchDetect : byte { Manual, BossPosition, MyStatus }

public enum Compass : byte { N, NE, E, SE, S, SW, W, NW }

// A single test a branch can run against the live pull. Branches combine these
// (all/any) so one variant can be "I have debuff X AND I'm a tank", another
// "tether 369 is on me", etc.
public enum CondKind : byte { MyStatus, MyRole, BossSide, TetherOnMe }

public enum RoleCat : byte { Tank, Healer, Dps, Melee, Ranged }

public sealed class StratCondition
{
    public CondKind Kind   { get; set; } = CondKind.MyStatus;
    public bool     Negate { get; set; }

    public uint   StatusId   { get; set; }
    public string StatusName { get; set; } = "";

    public RoleCat Role { get; set; } = RoleCat.Tank;

    public Compass BossSide { get; set; } = Compass.N;
    public uint    BossId   { get; set; }
    public string  BossName { get; set; } = "";

    public uint   TetherId   { get; set; }
    public string TetherName { get; set; } = "";

    public StratCondition Clone() => (StratCondition)MemberwiseClone();
}

// Where a role's marker + leash land. Fixed is a typed/clicked arena spot.
// TetheredToMe makes the marker stick to whatever the player is tethered to (the
// clone), so the leash points at YOUR pickup instead of a placebo coordinate.
public enum SpotAnchor : byte { Fixed, TetheredToMe }

public sealed class RoleSpot
{
    public StratRole Role     { get; set; } = StratRole.MT;
    public bool      Enabled  { get; set; } = true;
    public Vector3   Position { get; set; } = new(100f, 0f, 100f);

    public SpotAnchor Anchor   { get; set; } = SpotAnchor.Fixed;
    public uint       TetherId { get; set; }   // 0 = any tether on me

    public QuickShape Shape  { get; set; } = QuickShape.Circle;
    public Vector4    Color  { get; set; } = new(1f, 0.55f, 0.10f, 0.45f);
    public float      Radius { get; set; } = 1.5f;

    public bool    ShowLeash  { get; set; } = true;
    public Vector4 LeashColor { get; set; } = new(0.3f, 0.85f, 1f, 0.6f);

    public float Duration { get; set; } = 8f;

    public RoleSpot Clone() => (RoleSpot)MemberwiseClone();
}

public sealed class StratBranch
{
    public string Id   { get; set; } = Guid.NewGuid().ToString("N");
    public string Name { get; set; } = "Variant";

    public BranchDetect Detect { get; set; } = BranchDetect.Manual;

    public string  BossName { get; set; } = "";
    public uint    BossId   { get; set; }
    public Compass BossSide { get; set; } = Compass.S;

    public uint   StatusId   { get; set; }
    public string StatusName { get; set; } = "";

    // New rich path: combine any number of conditions. When non-empty this wins
    // over the legacy Detect field. Empty = catch-all/default branch.
    public bool RequireAll { get; set; } = true;
    public List<StratCondition> Conditions { get; set; } = new();

    public List<RoleSpot> Spots { get; set; } = new();

    public StratBranch Clone()
    {
        var c = (StratBranch)MemberwiseClone();
        c.Spots = Spots.ConvertAll(x => x.Clone());
        c.Conditions = Conditions.ConvertAll(x => x.Clone());
        return c;
    }
}

public sealed class StratSlide
{
    public string Id   { get; set; } = Guid.NewGuid().ToString("N");
    public string Name { get; set; } = "New step";

    public TriggerMatch On        { get; set; } = TriggerMatch.Cast;
    public string       Pattern   { get; set; } = "";
    public uint         DataId    { get; set; }
    public bool         MatchById { get; set; }

    public float DelaySeconds { get; set; }

    public List<StratBranch> Branches { get; set; } = new();

    public StratSlide Clone()
    {
        var c = (StratSlide)MemberwiseClone();
        c.Branches = Branches.ConvertAll(b => b.Clone());
        return c;
    }
}

public sealed class StratPack
{
    public string Id        { get; set; } = Guid.NewGuid().ToString("N");
    public bool   Enabled   { get; set; } = true;
    public string Name      { get; set; } = "New strat";
    public string FightKey  { get; set; } = "";
    public uint   Territory { get; set; }
    public string Author    { get; set; } = "";
    public bool   BuiltIn   { get; set; }

    public byte  ArenaShape   { get; set; }          // 0 circle, 1 square
    public float ArenaRadius  { get; set; } = 20f;
    public float ArenaCenterX { get; set; } = 100f;
    public float ArenaCenterZ { get; set; } = 100f;

    public List<StratSlide> Slides { get; set; } = new();

    public StratPack Clone()
    {
        var c = (StratPack)MemberwiseClone();
        c.Slides = Slides.ConvertAll(s => s.Clone());
        return c;
    }
}
