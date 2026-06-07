using System.Collections.Generic;
using System.Linq;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Enum;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;
using YapYapDraw.Engine.Vfx;

namespace YapYapDraw.Modules.Fight_92547;

public class KingdomBolt : ISpecialAction
{
    public override string Name => "Kingdom Bolt";

    public override HashSet<uint> ActionID => new HashSet<uint> { 43084u, 43085u, 43446u, 43447u };

    public override IEnumerable<StaticVfx> ActiveAOEs => aoes.Take(1);

    public override void OnActionCast(ActorCastInfo info)
    {
        switch (info.ActionId)
        {
        case 43084:
        case 43446:
            aoes.Add(SimpleElement.Circle(info.Pos, 8f, 3000f, 0f, new HitCounter
            {
                ActionID = new HashSet<uint> { info.ActionId }
            }, GroundOmen.Yellow));
            break;
        case 43085:
        case 43447:
            aoes.Add(SimpleElement.Donut(info.Pos, 8f, 24f, 3000f, 0f, new HitCounter
            {
                ActionID = new HashSet<uint> { info.ActionId }
            }, GroundOmen.Yellow));
            break;
        }
    }

    public override void OnAbilityCast(ActorAbilityInfo info)
    {
        if (aoes.Count > 0)
        {
            aoes.RemoveAt(0);
        }
    }
}
