using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.TenderValley;

public class NeedleStorm : ISpecialAction
{
    public override string Name => "Needle Storm";

    public override HashSet<uint> ActionID => new HashSet<uint> { 37388u, 37389u };

    public override uint Phase => 1u;

    public override void OnActionCast(ActorCastInfo info)
    {
        switch (info.ActionId)
        {
        case 37388:
            SimpleElement.Circle(info, 6f);
            break;
        case 37389:
            SimpleElement.Circle(info, 11f);
            break;
        }
    }
}
