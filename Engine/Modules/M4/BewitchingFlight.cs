using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.M4;

public class BewitchingFlight : ISpecialAction
{
    public override string Name => "Bewitching Flight";

    public override HashSet<uint> ActionID => new HashSet<uint> { 37560u };

    public override void OnActionCast(ActorCastInfo info)
    {
        SimpleElement.Rectangle(info);
    }
}
