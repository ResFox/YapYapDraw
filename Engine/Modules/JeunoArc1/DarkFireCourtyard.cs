using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.JeunoArc1;

public class DarkFireCourtyard : ISpecialAction
{
    public override string Name => "Dark Fire (courtyard)";

    public override HashSet<uint> ActionID => new HashSet<uint> { 40780u };

    public override uint Phase => 4u;

    public override void OnActionCast(ActorCastInfo info)
    {
        SimpleElement.Circle(info);
    }
}
