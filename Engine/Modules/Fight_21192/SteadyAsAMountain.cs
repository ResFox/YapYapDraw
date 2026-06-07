using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.Fight_21192;

public class SteadyAsAMountain : ISpecialAction
{
    public override string Name => "Steady as a Mountain";

    public override HashSet<uint> ActionID => new HashSet<uint> { 40603u };

    public override uint Phase => 3u;

    public override void OnActionCast(ActorCastInfo info)
    {
        SimpleElement.Donut(info, 15f, 35f);
    }
}
