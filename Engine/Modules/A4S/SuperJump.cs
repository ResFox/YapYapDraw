using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.A4S;

public class SuperJump : ISpecialAction
{
    public override string Name => "Super Jump";

    public override HashSet<uint> ActionID => new HashSet<uint> { 5968u };

    public override void OnActionCast(ActorCastInfo info)
    {
        SimpleElement.Circle(info.TargetId, 10f, 3000f, 0f, info.ActionId);
    }
}
