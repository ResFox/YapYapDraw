using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.UnendingCoil;

public class Cauterize : ISpecialAction
{
    public override string Name => "Cauterize";

    public override uint Phase => 2u;

    public override HashSet<uint> ActionID => new HashSet<uint> { 9931u, 9932u, 9933u, 9934u, 9935u };

    public override void OnActionCast(ActorCastInfo info)
    {
        SimpleElement.Rectangle(info, 52f, 10f);
    }
}
