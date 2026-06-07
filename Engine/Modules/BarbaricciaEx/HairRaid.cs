using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.BarbaricciaEx;
public class HairRaid : ISpecialAction
{
    public override string Name => "Hair Raid";

    public override HashSet<uint> ActionID => new HashSet<uint> { 30077u, 30079u };

    public override void OnActionCast(ActorCastInfo info)
    {
        switch (info.ActionId)
        {
        case 30077:
            SimpleElement.Fan(info, 40f, 120);
            break;
        case 30079:
            SimpleElement.Donut(info, 6f, 20f);
            break;
        }
    }
}
