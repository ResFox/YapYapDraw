using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.TheMesoTerminal;

public class FlayingFlail : ISpecialAction
{
    public override string Name => "Flaying Flail";

    public override HashSet<uint> ActionID => new HashSet<uint> { 43592u };

    public override uint Phase => 2u;

    public override void OnActionCast(ActorCastInfo info)
    {
        SimpleElement.Circle(info, 5f);
    }
}
