using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.M7;

public class BrutishSwing : ISpecialAction
{
    public override string Name => "BrutishSwing";

    public override uint Phase => 1u;

    public override HashSet<uint> ActionID => new HashSet<uint> { 42271u, 42270u, 42293u, 42295u, 42302u, 42303u, 42317u, 42319u };

    public override void OnActionCast(ActorCastInfo info)
    {
        bool donut;
        switch (info.ActionId)
        {
        case 42271:
        case 42295:
        case 42303:
        case 42319:
            donut = true;
            break;
        default:
            donut = false;
            break;
        }
        if (donut)
        {
            SimpleElement.Donut(info);
        }
        else
        {
            SimpleElement.Circle(info);
        }
    }
}
