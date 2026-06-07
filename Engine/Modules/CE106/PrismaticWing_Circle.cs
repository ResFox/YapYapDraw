using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.CE106;

public class PrismaticWing_Circle : ISpecialAction
{
    public override string Name => "PrismaticWing - Circle";

    public override HashSet<uint> ActionID => new HashSet<uint> { 42768u, 42766u };

    public override void OnActionCast(ActorCastInfo info)
    {
        SimpleElement.Circle(info);
    }
}
