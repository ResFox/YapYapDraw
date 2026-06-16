using System;
using System.Numerics;
using YapYapDraw.QuickDraws;

namespace YapYapDraw.Strats;

// Worked example built from the real M12S Idyllic Dream ids/positions, used to
// show the full feature set in the editor: per-role spots, boss-position splits,
// status conditions, tether conditions and role conditions.
public static class StratLibrary
{
    private static readonly Vector4 Defa  = new(0.45f, 0.40f, 1f, 0.55f);
    private static readonly Vector4 Stack = new(0.10f, 0.75f, 0.40f, 0.55f);
    private static readonly Vector4 Safe  = new(0.20f, 0.90f, 1f, 0.55f);
    private static readonly Vector4 Tower = new(0.20f, 0.95f, 0.35f, 0.60f);
    private static readonly Vector4 Bait  = new(1f, 0.80f, 0.10f, 0.45f);

    public static StratPack BuildIdyllicExample(uint territory)
    {
        var pack = new StratPack
        {
            Name       = "M12S Idyllic Dream (example)",
            Territory  = territory,
            Author     = "Res",
            ArenaShape = 0,
            ArenaRadius = 20f,
            ArenaCenterX = 100f,
            ArenaCenterZ = 100f,
        };

        pack.Slides.Add(SpreadStep());
        pack.Slides.Add(ConeSplitStep());
        pack.Slides.Add(TetherStep());
        pack.Slides.Add(TowerRoleStep());
        pack.Slides.Add(NearFarStep());

        return pack;
    }

    // Step 1 — opener spread: every role gets its own clock spot. Pure "all roles
    // in one strat", one branch, no conditions.
    private static StratSlide SpreadStep()
    {
        var s = NewSlide("Idyllic Dream — spread", 46345u);
        var b = new StratBranch { Name = "Spread" };

        Vector3[] clock =
        {
            new(100, 0, 86), new(110, 0, 90), new(114, 0, 100), new(110, 0, 110),
            new(100, 0, 114), new(90, 0, 110), new(86, 0, 100),  new(90, 0, 90),
        };
        for (int i = 0; i < 8; i++)
            b.Spots.Add(Spot((StratRole)i, clock[i], QuickShape.Circle, Safe, 1.2f));

        s.Branches.Add(b);
        return s;
    }

    // Step 2 — cone cleave: spot depends on which side the boss is. Boss-position
    // condition picks north vs south.
    private static StratSlide ConeSplitStep()
    {
        var s = NewSlide("Cone cleave — boss N/S", 46352u);

        var north = new StratBranch { Name = "Boss North" };
        north.Conditions.Add(new StratCondition { Kind = CondKind.BossSide, BossSide = Compass.N });
        FillUniform(north, new Vector3(100, 0, 110), QuickShape.Circle, Safe, 1.5f); // stand south, away from north boss

        var south = new StratBranch { Name = "Boss South" };
        south.Conditions.Add(new StratCondition { Kind = CondKind.BossSide, BossSide = Compass.S });
        FillUniform(south, new Vector3(100, 0, 90), QuickShape.Circle, Safe, 1.5f);

        var def = new StratBranch { Name = "Default" };
        FillUniform(def, new Vector3(100, 0, 100), QuickShape.Circle, Safe, 1.5f);

        s.Branches.Add(north);
        s.Branches.Add(south);
        s.Branches.Add(def);
        return s;
    }

    // Step 3 — defamation/stack: when a tether forms, branch off which one is on
    // you and leash to the actual clone you're tethered to (no fixed placebo).
    private static StratSlide TetherStep()
    {
        var s = NewSlide("Tether resolve — grab my clone", 0u);
        s.On        = TriggerMatch.Tether;
        s.MatchById = false;

        var defa = new StratBranch { Name = "Defamation on me" };
        defa.Conditions.Add(new StratCondition { Kind = CondKind.TetherOnMe, TetherId = 368u, TetherName = "Defamation" });
        FillTether(defa, 368u, QuickShape.Donut, Defa, 4f);

        var stack = new StratBranch { Name = "Stack on me" };
        stack.Conditions.Add(new StratCondition { Kind = CondKind.TetherOnMe, TetherId = 369u, TetherName = "Stack" });
        FillTether(stack, 369u, QuickShape.Circle, Stack, 3f);

        s.Branches.Add(defa);
        s.Branches.Add(stack);
        return s;
    }

    // Step 4 — towers: light goes to one tower, non-light to the other. Branch by
    // role.
    private static StratSlide TowerRoleStep()
    {
        var s = NewSlide("Towers — light / non-light", 46367u);

        var dps = new StratBranch { Name = "DPS (light)" };
        dps.Conditions.Add(new StratCondition { Kind = CondKind.MyRole, Role = RoleCat.Dps });
        FillUniform(dps, new Vector3(81.757f, 0, 95.757f), QuickShape.Tower, Tower, 2.5f);

        var th = new StratBranch { Name = "Tank/Healer (non-light)" };
        th.RequireAll = false;
        th.Conditions.Add(new StratCondition { Kind = CondKind.MyRole, Role = RoleCat.Tank });
        th.Conditions.Add(new StratCondition { Kind = CondKind.MyRole, Role = RoleCat.Healer });
        FillUniform(th, new Vector3(90.243f, 0, 95.757f), QuickShape.Tower, Tower, 2.5f);

        s.Branches.Add(dps);
        s.Branches.Add(th);
        return s;
    }

    // Step 5 — near/far bait: branch by the light debuff you're given.
    private static StratSlide NearFarStep()
    {
        var s = NewSlide("Near / Far bait", 46324u);

        var far = new StratBranch { Name = "Given Far" };
        far.Conditions.Add(new StratCondition { Kind = CondKind.MyStatus, StatusId = 4766u, StatusName = "Given Far" });
        FillUniform(far, new Vector3(110.152f, 0, 98.237f), QuickShape.Circle, Bait, 2f);

        var near = new StratBranch { Name = "Given Near" };
        near.Conditions.Add(new StratCondition { Kind = CondKind.MyStatus, StatusId = 4767u, StatusName = "Given Near" });
        FillUniform(near, new Vector3(106.973f, 0, 94.048f), QuickShape.Circle, Bait, 2f);

        var def = new StratBranch { Name = "Default (taken)" };
        FillUniform(def, new Vector3(114.708f, 0, 109.144f), QuickShape.Circle, Bait, 2f);

        s.Branches.Add(far);
        s.Branches.Add(near);
        s.Branches.Add(def);
        return s;
    }

    private static StratSlide NewSlide(string name, uint actionId) => new()
    {
        Name      = name,
        On        = TriggerMatch.Cast,
        MatchById = true,
        DataId    = actionId,
        Pattern   = name,
    };

    private static RoleSpot Spot(StratRole role, Vector3 pos, QuickShape shape, Vector4 color, float radius) => new()
    {
        Role     = role,
        Position = pos,
        Shape    = shape,
        Color    = color,
        Radius   = radius,
        Duration = 10f,
        ShowLeash = true,
    };

    private static void FillUniform(StratBranch b, Vector3 pos, QuickShape shape, Vector4 color, float radius)
    {
        for (int i = 0; i < 8; i++)
            b.Spots.Add(Spot((StratRole)i, pos, shape, color, radius));
    }

    // Every role leashes to the live clone tethered to them, filtered by tether id.
    private static void FillTether(StratBranch b, uint tetherId, QuickShape shape, Vector4 color, float radius)
    {
        for (int i = 0; i < 8; i++)
        {
            var sp = Spot((StratRole)i, new Vector3(100, 0, 100), shape, color, radius);
            sp.Anchor   = SpotAnchor.TetheredToMe;
            sp.TetherId = tetherId;
            b.Spots.Add(sp);
        }
    }
}
