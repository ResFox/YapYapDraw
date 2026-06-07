using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.Fight_56879;

public class RisingDragonBeacon : ISpecialAction
{
    public override string Name => "Rising Dragon Beacon";

    public override HashSet<uint> ActionID => new HashSet<uint> { 33935u };

    public override uint Phase => 2u;

    public override void OnActionCast(ActorCastInfo info)
    {
        SimpleElement.Circle(info, 10f);
    }
}
