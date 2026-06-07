using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;
using YapYapDraw.Engine.Util;

namespace YapYapDraw.Modules.CenoteJaJa;

public class FlameSwordCrossFlameSword : ISpecialAction
{
    public override string Name => "Flame Sword / Cross Flame Sword";

    public override HashSet<uint> ActionID => new HashSet<uint> { 38251u, 38255u };

    public override void OnActionCast(ActorCastInfo info)
    {
        uint sourceId = info.SourceId;
        Angle facing = info.Facing;
        float castTime = info.CastTime * 1000f;
        SimpleElement.Rectangle(sourceId, 20f, 2.5f, 20f, null, facing, castTime);
    }
}
