using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.UWU;

public class GeocrushTransition : ISpecialAction
{
    public override string Name => "Geocrush (transition)";

    public override uint Phase => 3u;

    public override HashSet<uint> ActionID => new HashSet<uint> { 11517u };

    public override void OnActionCast(ActorCastInfo info)
    {
        SimpleElement.Circle(info, 18f);
    }
}
