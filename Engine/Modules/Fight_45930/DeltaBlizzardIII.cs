using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.Fight_45930;

public class DeltaBlizzardIII : ISpecialAction
{
    public override string Name => "Delta Blizzard III";

    public override HashSet<uint> ActionID => new HashSet<uint> { 25268u };

    public override uint Phase => 3u;

    public override void OnActionCast(ActorCastInfo info)
    {
        SimpleElement.Circle(info.Pos, 15f, 3000f, 0f, new HitCounter
        {
            ActionID = new HashSet<uint> { info.ActionId }
        });
    }
}
