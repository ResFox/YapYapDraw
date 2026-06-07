using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.Fight_21192;

public class DancingStone : ISpecialAction
{
    public override string Name => "Dancing Stone";

    public override HashSet<uint> ActionID => new HashSet<uint> { 40607u, 40608u };

    public override uint Phase => 3u;

    public override void OnActionCast(ActorCastInfo info)
    {
        SimpleElement.Circle(info.Pos, 7f, 3000f, 0f, new HitCounter
        {
            ActionID = new HashSet<uint> { info.ActionId }
        });
    }
}
