using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;
using YapYapDraw.Engine.Util;

namespace YapYapDraw.Modules.Fight_56456;

public class NegativeMemory : ISpecialAction
{
    public override string Name => "Negative Memory";

    public override HashSet<uint> ActionID => new HashSet<uint> { 37340u };

    public override uint Phase => 3u;

    public override void OnActionCast(ActorCastInfo info)
    {
        uint sourceId = info.SourceId;
        float castTime = info.CastTime * 1000f;
        SimpleElement.Rectangle(sourceId, 40f, 2f, 0f, null, default, castTime);
    }
}
