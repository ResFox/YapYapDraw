using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.M8S;

public class Mooncleaver : ISpecialAction
{
    public override string Name => "Mooncleaver";

    public override uint Phase => 2u;

    public override HashSet<uint> ActionID => new HashSet<uint> { 42086u, 42829u };

    public override void OnActionCast(ActorCastInfo info)
    {
        SimpleElement.Circle(info);
    }
}
