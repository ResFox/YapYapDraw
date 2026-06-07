using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.M10S;

public class MixedBlast : ISpecialAction
{
    public override string Name => "Mixed Blast";

    public override HashSet<uint> ActionID => new HashSet<uint> { 46587u };

    public override void OnActionCast(ActorCastInfo info)
    {
        SimpleElement.Circle(info);
    }
}
