using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Util;

namespace YapYapDraw.Modules.E2;

public class RagingStorm : ISpecialAction
{
    public override string Name => "Raging Storm";

    public override HashSet<uint> ActionID => new HashSet<uint>();

    public override void OnActorTetherEvent(uint actorId, uint Id, ulong targetId)
    {
        if (Id == 109)
        {
            SimpleElement.FanToTarget(targetId, actorId, 40f, 45, Follow: true, default, 3000f, 19425u);
        }
    }
}
