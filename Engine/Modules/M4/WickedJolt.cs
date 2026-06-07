using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.M4;

public class WickedJolt : ISpecialAction
{
    public override string Name => "Wicked Jolt";

    public override HashSet<uint> ActionID => new HashSet<uint> { 37576u };

    public override void OnActionCast(ActorCastInfo info)
    {
        SimpleElement.RectangleToTarget(info, 60f, 2.5f);
    }
}
