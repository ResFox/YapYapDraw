using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.M7S;

public class ItCameFromTheDirt : ISpecialAction
{
    public override string Name => "It Came From the Dirt";

    public override HashSet<uint> ActionID => new HashSet<uint> { 42362u };

    public override void OnActionCast(ActorCastInfo info)
    {
        SimpleElement.Circle(info.Pos, 6f, 3000f, 0f, new HitCounter
        {
            ActionID = new HashSet<uint> { info.ActionId }
        });
    }
}
