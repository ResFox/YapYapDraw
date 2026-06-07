using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.M5S;

public class GetDown : ISpecialAction
{
    public override string Name => "GetDown! Bait";

    public override HashSet<uint> ActionID => new HashSet<uint> { 42853u };

    public override void OnActionCast(ActorCastInfo info)
    {
        SimpleElement.Fan(info.SourceId, 40f, 45, info.Facing, info.CastTime * 1000f, 0f, 0u);
    }
}
