using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.UWU;

public class Eruption : ISpecialAction
{
    public override string Name => "Eruption";

    public override uint Phase => 2u;

    public override HashSet<uint> ActionID => new HashSet<uint> { 11098u };

    public override void OnActionCast(ActorCastInfo info)
    {
        SimpleElement.Circle(info.Pos, 8f);
    }
}
