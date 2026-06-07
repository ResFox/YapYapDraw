using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.E2;

public class BurstFlame : ISpecialAction
{
    public override string Name => "Burst Flame";

    public override HashSet<uint> ActionID => new HashSet<uint> { 19437u };

    public override uint Phase => 2u;

    public override void OnActionCast(ActorCastInfo info)
    {
        SimpleElement.Circle(info.TargetId, 4f, 3000f, 0f, info.ActionId);
    }
}
