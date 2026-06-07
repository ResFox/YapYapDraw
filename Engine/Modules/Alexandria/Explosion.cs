using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;
using YapYapDraw.Engine.Util;

namespace YapYapDraw.Modules.Alexandria;

public class Explosion : ISpecialAction
{
    public override string Name => "Explosion";

    public override uint Phase => 3u;

    public override HashSet<uint> ActionID => new HashSet<uint> { 39239u };

    public override void OnActionCast(ActorCastInfo info)
    {
        uint sourceId = info.SourceId;
        Angle facing = info.Facing;
        float castTime = info.CastTime * 1000f;
        SimpleElement.Rectangle(sourceId, 50f, 4f, 50f, null, facing, castTime);
    }
}
