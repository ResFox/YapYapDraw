using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;
using YapYapDraw.Engine.Util;

namespace YapYapDraw.Modules.M5S;

public class QuarterBeats : ISpecialAction
{
    public override string Name => "Quarter Beats";

    public override HashSet<uint> ActionID => new HashSet<uint> { 42844u };

    public override void OnActionCast(ActorCastInfo info)
    {
        SimpleLockon.ShareLockon2(info.TargetId.GameObject());
    }
}
