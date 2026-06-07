using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;
using YapYapDraw.Engine.Util;

namespace YapYapDraw.Modules.FRU;

public class Tower : ISpecialAction
{
    public override string Name => "Tower";

    public override uint Phase => 1u;

    public override HashSet<uint> ActionID => new HashSet<uint> { 40161u, 40162u, 40163u, 40164u, 40129u, 40130u, 40132u, 40133u, 40134u };

    public override void OnActionCast(ActorCastInfo info)
    {
        bool isThinLine;
        switch (info.ActionId)
        {
        case 40129:
        case 40133:
        case 40161:
        case 40163:
            isThinLine = true;
            break;
        default:
            isThinLine = false;
            break;
        }
        if (isThinLine)
        {
            ActorCastInfo castInfo = info;
            float delay = ((base.NumCasts == 0) ? ((info.CastTime - 3f) * 1000f) : 0f);
            SimpleElement.Rectangle(castInfo, 50f, 5f, 50f, null, delay);
            base.NumCasts++;
            return;
        }
        ushort actionId = info.ActionId;
        if (actionId == 40134 || actionId == 40164)
        {
            ActorCastInfo castInfo = info;
            float delay = (info.CastTime - 3f) * 1000f;
            SimpleElement.Rectangle(castInfo, 50f, 10f, 50f, null, delay);
        }
        else
        {
            SimpleElement.RectangleKnockBack2(info.SourceId.GameObject().Position, 50f, 30f, 50f, 0.Degrees(), 3000f, (info.CastTime - 2f) * 1000f, new HitCounter
            {
                ActionID = new HashSet<uint> { info.ActionId },
                TargetHitCount = 1
            });
        }
    }
}
