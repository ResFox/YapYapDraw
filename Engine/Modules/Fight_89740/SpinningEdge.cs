using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.Fight_89740;

public class SpinningEdge : ISpecialAction
{
    public override string Name => "Spinning Edge";

    public override HashSet<uint> ActionID => new HashSet<uint> { 37734u, 37735u };

    public override uint Phase => 1u;

    public override void OnActionCast(ActorCastInfo info)
    {
        switch (info.ActionId)
        {
        case 37734:
            SimpleElement.Donut(info, 10f, 30f);
            break;
        case 37735:
            SimpleElement.Circle(info);
            break;
        }
    }
}
