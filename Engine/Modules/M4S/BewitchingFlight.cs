using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.M4S;

public class BewitchingFlight : ISpecialAction
{
    public override string Name => "Bewitching Flight";

    public override uint Phase => 1u;

    public override HashSet<uint> ActionID => new HashSet<uint> { 38377u };

    public override void OnActionCast(ActorCastInfo info)
    {
        SimpleElement.Rectangle(info, 40f, 2.5f);
    }
}
