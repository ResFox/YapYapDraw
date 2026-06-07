using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.Fight_33798;

public class WyrmResonance : ISpecialAction
{
    public override string Name => "Wyrm Resonance";

    public override HashSet<uint> ActionID => new HashSet<uint> { 43836u, 45070u };

    public override void OnActionCast(ActorCastInfo info)
    {
        SimpleElement.Rectangle(info);
    }
}
