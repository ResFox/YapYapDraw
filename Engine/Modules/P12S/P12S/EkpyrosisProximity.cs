using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.P12S.P12S;

public class EkpyrosisProximity : ISpecialAction
{
    public override string Name => "Flare";

    public override uint Phase => 2u;

    public override HashSet<uint> ActionID => new HashSet<uint> { 33569u, 34435u };

    public override void OnActionCast(ActorCastInfo info)
    {
        SimpleElement.Circle(info, 18f);
    }
}
