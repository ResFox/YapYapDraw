using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.P12S.P12S;

public class Implode : ISpecialAction
{
    public override string Name => "Implode";

    public override uint Phase => 2u;

    public override HashSet<uint> ActionID => new HashSet<uint> { 33587u };

    public override void OnActionCast(ActorCastInfo info)
    {
        SimpleElement.Circle(info, 4f);
    }
}
