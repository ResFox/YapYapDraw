using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.M4;

public class WitchHunt : ISpecialAction
{
    public override string Name => "Witch Hunt";

    public override HashSet<uint> ActionID => new HashSet<uint> { 37557u };

    public override void OnActionCast(ActorCastInfo info)
    {
        base.NumCasts++;
        SimpleElement.Circle(info.Pos, 6f, 3000f, 0f, new HitCounter
        {
            ActionID = new HashSet<uint> { 37558u },
            TargetHitCount = base.NumCasts
        });
    }
}
