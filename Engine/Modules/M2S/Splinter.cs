using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.M2S;

public class Splinter : ISpecialAction
{
    public override string Name => "Splinter";

    public override uint Phase => 2u;

    public override HashSet<uint> ActionID => new HashSet<uint> { 37256u, 39691u };

    public override void OnActionCast(ActorCastInfo info)
    {
        SimpleElement.Circle(info, 8f);
    }
}
