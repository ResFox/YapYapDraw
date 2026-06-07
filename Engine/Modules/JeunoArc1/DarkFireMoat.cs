using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.JeunoArc1;

public class DarkFireMoat : ISpecialAction
{
    public override string Name => "Dark Fire (moat)";

    public override HashSet<uint> ActionID => new HashSet<uint> { 40781u };

    public override uint Phase => 4u;

    public override void OnActionCast(ActorCastInfo info)
    {
        SimpleElement.Donut(info);
    }
}
