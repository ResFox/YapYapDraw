using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.P1NHaunted;

public class Freeze : ISpecialAction
{
    public override string Name => "Freeze";

    public override HashSet<uint> ActionID => new HashSet<uint> { 33057u };

    public override void OnActionCast(ActorCastInfo info)
    {
        SimpleElement.Donut(info, 8f, 30f);
    }
}
