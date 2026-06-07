using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.CE107;

public class ScathingSweep : ISpecialAction
{
    public override string Name => "ScathingSweep";

    public override HashSet<uint> ActionID => new HashSet<uint> { 42691u };

    public override void OnActionCast(ActorCastInfo info)
    {
        SimpleElement.Rectangle(info);
    }
}
