using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.JeunoArc1;

public class WingsOfTerror : ISpecialAction
{
    public override string Name => "Wings of Terror";

    public override HashSet<uint> ActionID => new HashSet<uint> { 40848u };

    public override uint Phase => 2u;

    public override void OnActionCast(ActorCastInfo info)
    {
        SimpleElement.Rectangle(info);
    }
}
