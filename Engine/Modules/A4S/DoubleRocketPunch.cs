using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.A4S;

public class DoubleRocketPunch : ISpecialAction
{
    public override string Name => "Double Rocket Punch";

    public override HashSet<uint> ActionID => new HashSet<uint> { 5966u };

    public override void OnActionCast(ActorCastInfo info)
    {
        SimpleElement.Circle(info.TargetId, 3f, 3000f, 0f, info.ActionId);
    }
}
