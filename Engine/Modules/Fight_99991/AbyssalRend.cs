using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.Fight_99991;

public class AbyssalRend : ISpecialAction
{
    public override string Name => "Abyssal Rend";

    public override HashSet<uint> ActionID => new HashSet<uint> { 35628u, 35629u, 35630u, 35631u, 35632u };

    public override uint Phase => 2u;

    public override void OnActionCast(ActorCastInfo info)
    {
        switch (info.ActionId)
        {
        case 35628:
        case 35629:
            SimpleElement.Fan(info, 60f, 40);
            break;
        case 35630:
            SimpleElement.Fan(info, 60f, 40, 7700f);
            break;
        case 35631:
            SimpleElement.Fan(info, 60f, 40, 8400f);
            break;
        case 35632:
            SimpleElement.Fan(info, 60f, 40, 9100f);
            break;
        }
    }
}
