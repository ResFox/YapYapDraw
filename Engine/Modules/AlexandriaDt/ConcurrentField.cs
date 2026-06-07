using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.AlexandriaDt;

public class ConcurrentField : ISpecialAction
{
    public override string Name => "Concurrent Field";

    public override HashSet<uint> ActionID => new HashSet<uint> { 42521u };

    public override void OnActionCast(ActorCastInfo info)
    {
        SimpleElement.Fan(info.Pos, 26f, 50, info.Facing, 3000f, 0f, new HitCounter
        {
            ActionID = ActionID
        });
    }
}
