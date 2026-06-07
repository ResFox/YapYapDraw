using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.M7S;

public class RootsOfEvil : ISpecialAction
{
    public override string Name => "Roots of Evil";

    public override HashSet<uint> ActionID => new HashSet<uint> { 42354u };

    public override void OnActionCast(ActorCastInfo info)
    {
        SimpleElement.Circle(info.Pos, 12f);
    }
}
