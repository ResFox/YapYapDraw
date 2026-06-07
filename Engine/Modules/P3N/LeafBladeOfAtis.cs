using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.P3N;

public class LeafBladeOfAtis : ISpecialAction
{
    public override string Name => "Leaf Blade of Atis";

    public override HashSet<uint> ActionID => new HashSet<uint> { 30725u };

    public override void OnActionCast(ActorCastInfo info)
    {
        SimpleElement.LineCircle(info, 7f, 1300f, 8);
    }
}
