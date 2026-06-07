using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.E11;

public class Thunderburst : ISpecialAction
{
    public override string Name => "Thunderburst";

    public override HashSet<uint> ActionID => new HashSet<uint> { 22063u, 22086u };

    public override uint Phase => 1u;

    public override void OnActionCast(ActorCastInfo info)
    {
        SimpleElement.Rectangle(info, 40f, 10f, 40f);
    }
}
