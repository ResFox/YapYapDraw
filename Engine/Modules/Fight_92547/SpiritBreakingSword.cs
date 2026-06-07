using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.Fight_92547;

public class SpiritBreakingSword : ISpecialAction
{
    public override string Name => "Spirit-breaking Sword";

    public override HashSet<uint> ActionID => new HashSet<uint> { 43129u };

    public override void OnActionCast(ActorCastInfo info)
    {
        SimpleElement.FanToTarget(info, 50f, 45);
    }
}
