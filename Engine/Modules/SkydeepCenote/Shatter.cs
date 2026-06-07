using System.Collections.Generic;
using System.Numerics;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;
using YapYapDraw.Engine.Util;

namespace YapYapDraw.Modules.SkydeepCenote;

public class Shatter : ISpecialAction
{
    public override string Name => "Shatter";

    public override uint Phase => 3u;

    public override HashSet<uint> ActionID => new HashSet<uint> { 36679u, 36680u, 36681u, 36682u };

    public override void OnActionCast(ActorCastInfo info)
    {
        switch (info.ActionId)
        {
        case 36679:
        case 36680:
            SimpleElement.Rectangle(info.SourceId, 40f, 10f, 0f, null, default, 3000f, 0f, 36684u);
            break;
        case 36681:
        case 36682:
            SimpleElement.Rectangle(new Vector3(91.564f, -192f, -453f), 45f, 11f, 0f, -17.Degrees(), 3000f, 0f, new HitCounter
            {
                ActionID = new HashSet<uint> { 36685u, 36686u }
            });
            SimpleElement.Rectangle(new Vector3(108.436f, -192f, -453f), 45f, 11f, 0f, 17.Degrees(), 3000f, 0f, new HitCounter
            {
                ActionID = new HashSet<uint> { 36685u, 36686u }
            });
            break;
        }
    }
}
