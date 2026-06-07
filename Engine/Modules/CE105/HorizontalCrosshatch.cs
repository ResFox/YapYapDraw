using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;
using YapYapDraw.Engine.Util;

namespace YapYapDraw.Modules.CE105;

public class HorizontalCrosshatch : ISpecialAction
{
    public override string Name => "HorizontalCrosshatch";

    public override HashSet<uint> ActionID => new HashSet<uint> { 41324u, 41331u };

    public override void OnActionCast(ActorCastInfo info)
    {
        SimpleElement.Fan(info.SourceId.GameObject(), 50f, 90, 90f.Degrees(), info.CastTime * 1000f);
        SimpleElement.Fan(info.SourceId.GameObject(), 50f, 90, -90f.Degrees(), info.CastTime * 1000f);
        SimpleElement.Fan(info.SourceId.GameObject(), 50f, 90, 0f.Degrees(), 2000f, info.CastTime * 1000f);
        SimpleElement.Fan(info.SourceId.GameObject(), 50f, 90, 180f.Degrees(), 2000f, info.CastTime * 1000f);
    }
}
