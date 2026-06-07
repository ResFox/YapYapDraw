using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.SanDoriaArc2;

public class SculptorSHandSlam : ISpecialAction
{
    public override string Name => "Sculptor's Hand (slam)";

    public override HashSet<uint> ActionID => new HashSet<uint> { 44442u, 44441u };

    public override void OnActionCast(ActorCastInfo info)
    {
        SimpleElement.Circle(info);
    }
}
