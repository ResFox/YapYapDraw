using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.Fight_56879;

public class SlowEarthquake : ISpecialAction
{
    public override string Name => "Slow Earthquake";

    public override HashSet<uint> ActionID => new HashSet<uint> { 33893u };

    public override uint Phase => 1u;

    public override void OnActionCast(ActorCastInfo info)
    {
        SimpleElement.Circle(info, 16f);
    }
}
