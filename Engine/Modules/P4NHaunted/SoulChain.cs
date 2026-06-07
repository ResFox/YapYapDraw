using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Util;

namespace YapYapDraw.Modules.P4NHaunted;

public class SoulChain : ISpecialAction
{
    public override string Name => "Soul Chain";

    public override HashSet<uint> ActionID => new HashSet<uint>();

    public override void OnActorTetherEvent(uint actorId, uint Id, ulong targetId)
    {
        if (Id == 232)
        {
            SimpleElement.Rectangle(actorId, 5f, 10f, 5f, null, default, 3000f, 0f, 33472u);
        }
    }
}
