using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.Fight_21192;

public class Dig : ISpecialAction
{
    public override string Name => "Dig";

    public override HashSet<uint> ActionID => new HashSet<uint> { 40605u };

    public override uint Phase => 3u;

    public override void OnActionCast(ActorCastInfo info)
    {
        SimpleElement.Circle(info.Pos, 11f, 3000f, 0f, new HitCounter
        {
            ActionID = new HashSet<uint> { info.ActionId }
        });
    }
}
