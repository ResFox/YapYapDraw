using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.JeunoArc1;

public class Explosion : ISpecialAction
{
    public override string Name => "Explosion";

    public override HashSet<uint> ActionID => new HashSet<uint> { 40955u };

    public override void OnActionCast(ActorCastInfo info)
    {
        SimpleElement.Circle(info.SourceId, 8f, info.CastTime * 1000f, 0f, 0u, null);
    }
}
