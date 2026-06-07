using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.Origenics;

public class Arrogance : ISpecialAction
{
    public override string Name => "Arrogance";

    public override uint Phase => 1u;

    public override HashSet<uint> ActionID => new HashSet<uint> { 36465u, 36466u, 36467u, 36469u, 36470u, 36471u };

    public override void OnActionCast(ActorCastInfo info)
    {
        switch (info.ActionId)
        {
        case 36465:
        case 36466:
            if (base.NumCasts < 2)
            {
                SimpleElement.Fan(info.SourceId, 25f, 210, info.Facing, 3000f, 0f, (info.ActionId == 36465) ? 36469u : 36470u);
            }
            else
            {
                ActionQueue.Enqueue((new HashSet<uint> { 36469u, 36470u, 36471u }, delegate
                {
                    SimpleElement.Fan(info.SourceId, 25f, 210, info.Facing, 3000f, 0f, (info.ActionId == 36465) ? 36469u : 36470u);
                }));
            }
            base.NumCasts++;
            break;
        case 36467:
            if (base.NumCasts < 2)
            {
                SimpleElement.Fan(info.SourceId, 25f, 90, info.Facing, 3000f, 0f, 36471u);
            }
            else
            {
                ActionQueue.Enqueue((new HashSet<uint> { 36469u, 36470u, 36471u }, delegate
                {
                    SimpleElement.Fan(info.SourceId, 25f, 90, info.Facing, 3000f, 0f, 36471u);
                }));
            }
            base.NumCasts++;
            break;
        }
    }

    public override void OnAbilityCast(ActorAbilityInfo info)
    {
        uint actionId = info.ActionId;
        if (actionId - 36469 <= 2)
        {
            base.NumCasts = 0;
        }
    }
}
