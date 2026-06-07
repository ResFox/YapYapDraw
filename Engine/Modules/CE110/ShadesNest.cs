using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.CE110;

public class ShadesNest : ISpecialAction
{
    public override string Name => "Shades' Nest";

    public override HashSet<uint> ActionID => new HashSet<uint> { 42033u };

    public override void OnActionCast(ActorCastInfo info)
    {
        SimpleElement.Donut(info, 7f, 50f);
    }
}
