using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;
using YapYapDraw.Engine.Util;

namespace YapYapDraw.Modules.M2;

public class BlindingLove : ISpecialAction
{
    public override string Name => "Blinding Love";

    public override uint Phase => 1u;

    public override HashSet<uint> ActionID => new HashSet<uint> { 39525u, 39526u };

    public override void OnActionCast(ActorCastInfo info)
    {
        if (info.ActionId == 39525)
        {
            uint sourceId = info.SourceId;
            Angle facing = info.Facing;
            SimpleElement.Rectangle(sourceId, 50f, 4f, 0f, null, facing, 4000f, 3000f);
        }
        if (info.ActionId == 39526)
        {
            SimpleElement.Rectangle(info, 50f, 4f);
        }
    }
}
