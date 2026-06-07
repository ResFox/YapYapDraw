using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;
using YapYapDraw.Engine.Util;

namespace YapYapDraw.Modules.M4;

public class Burst : ISpecialAction
{
    public override string Name => "Burst (lightning lines)";

    public override HashSet<uint> ActionID => new HashSet<uint> { 37561u };

    public override void OnActionCast(ActorCastInfo info)
    {
        uint sourceId = info.SourceId;
        Angle facing = info.Facing;
        float castTime = info.CastTime * 1000f;
        SimpleElement.Rectangle(sourceId, 40f, 8f, 0f, null, facing, castTime);
    }
}
