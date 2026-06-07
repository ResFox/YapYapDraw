using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;
using YapYapDraw.Engine.Util;

namespace YapYapDraw.Modules.BarbaricciaEx;
public class BrutalGust : ISpecialAction
{
    public override string Name => "Brutal Gust";

    public override HashSet<uint> ActionID => new HashSet<uint> { 30085u };

    public override void OnActionCast(ActorCastInfo info)
    {
        uint sourceId = info.SourceId;
        Angle facing = info.Facing;
        float castTime = info.CastTime * 1000f;
        SimpleElement.Rectangle(sourceId, 40f, 2f, 0f, null, facing, castTime);
    }
}
