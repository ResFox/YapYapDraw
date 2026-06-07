using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;
using YapYapDraw.Engine.Util;

namespace YapYapDraw.Modules.Fight_5529;

public class RoleSwap : ISpecialAction
{
    public override string Name => "Role Swap";

    public override HashSet<uint> ActionID => new HashSet<uint>();

    public override void OnAddStatus(ActorStatusChangeInfo info)
    {
        if (info.StatusID == 2273)
        {
            if (info.Stack == 436)
            {
                SimpleElement.Circle(info.SourceID, 10f, 3000f, 0f, 26055u);
            }
            else if (info.Stack == 437)
            {
                SimpleElement.Donut(info.SourceID, 5f, 40f, 3000f, 0f, 26056u);
            }
        }
    }

    public override void OnRemoveStatus(ActorStatusChangeInfo info)
    {
        if (info.StatusID == 2273)
        {
            SimpleElement.Cross(info.SourceID, 40f, 5f, default, 3000f, 0f, 26255u);
        }
    }
}
