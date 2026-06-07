using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.Fight_89740;

public class HalfCircuit : ISpecialAction
{
    public override string Name => "Half Circuit";

    public override HashSet<uint> ActionID => new HashSet<uint> { 37741u, 37742u, 37743u };

    public override uint Phase => 1u;

    public override void OnActionCast(ActorCastInfo info)
    {
        switch (info.ActionId)
        {
        case 37741:
            SimpleElement.Rectangle(info);
            break;
        case 37742:
            SimpleElement.Donut(info, 10f, 30f);
            break;
        case 37743:
            SimpleElement.Circle(info);
            break;
        }
    }
}
