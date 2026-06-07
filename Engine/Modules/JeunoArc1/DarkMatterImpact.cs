using System.Collections.Generic;
using System.Numerics;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.JeunoArc1;

public class DarkMatterImpact : ISpecialAction
{
    public override string Name => "Dark Matter Impact";

    public override HashSet<uint> ActionID => new HashSet<uint> { 40854u };

    public override uint Phase => 2u;

    public override void OnActionCast(ActorCastInfo info)
    {
        SimpleElement.Donut(new Vector3(-500f, -500f, 600f), 30f, 35f, 3000f, 0f, new HitCounter
        {
            ActionID = new HashSet<uint> { 40854u }
        });
    }
}
