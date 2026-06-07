using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.JeunoArc1;

public class DoomTwinBlades : ISpecialAction
{
    public override string Name => "Doom Twin Blades";

    public override HashSet<uint> ActionID => new HashSet<uint> { 41092u };

    public override uint Phase => 3u;

    public override void OnActionCast(ActorCastInfo info)
    {
        SimpleElement.Cross(info);
    }
}
