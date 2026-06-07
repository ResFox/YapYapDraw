using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;
using YapYapDraw.Engine.Util;

namespace YapYapDraw.Modules.CE107;

public class HeatedOutburst : ISpecialAction
{
    public override string Name => "HeatedOutburst";

    public override HashSet<uint> ActionID => new HashSet<uint> { 37804u };

    public override void OnActionCast(ActorCastInfo info)
    {
        SimpleElement.Circle(info.Pos, 13f, 3000f, 0f, new HitCounter
        {
            ActionID = new HashSet<uint> { 37804u },
            HitTarget = info.SourceId.GameObject()
        });
    }
}
