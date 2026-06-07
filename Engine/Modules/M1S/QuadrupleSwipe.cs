using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;
using YapYapDraw.Engine.Util;

namespace YapYapDraw.Modules.M1S;

public class QuadrupleSwipe : ISpecialAction
{
    public override string Name => "Quadruple Swipe";

    public override uint Phase => 1u;

    public override HashSet<uint> ActionID => new HashSet<uint> { 37982u, 38016u };

    public override void OnActionCast(ActorCastInfo info)
    {
        SimpleLockon.ShareLockon2(info.TargetId.GameObject());
    }
}
