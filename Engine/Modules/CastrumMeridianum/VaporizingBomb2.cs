using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.CastrumMeridianum;

public class VaporizingBomb2 : ISpecialAction
{
    public override string Name => "Vaporizing Bomb";

    public override HashSet<uint> ActionID => new HashSet<uint> { 29356u };

    public override uint Phase => 3u;

    public override void OnActionCast(ActorCastInfo info)
    {
        SimpleElement.Circle(info, 13f);
    }
}
