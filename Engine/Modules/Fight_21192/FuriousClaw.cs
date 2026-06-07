using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;
using YapYapDraw.Engine.Util;

namespace YapYapDraw.Modules.Fight_21192;

public class FuriousClaw : ISpecialAction
{
    public override string Name => "Furious Claw";

    public override HashSet<uint> ActionID => new HashSet<uint> { 40613u };

    public override uint Phase => 3u;

    public override void OnActionCast(ActorCastInfo info)
    {
        SimpleElement.Fan(info.SourceId.GameObject(), 45f, 180, info.Facing, 3000f, 0f, new HitCounter
        {
            ActionID = new HashSet<uint> { 40613u, 40614u },
            TargetHitCount = 6
        });
    }
}
