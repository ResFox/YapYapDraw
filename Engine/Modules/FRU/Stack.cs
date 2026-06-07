using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;
using YapYapDraw.Engine.Util;

namespace YapYapDraw.Modules.FRU;

public class Stack : ISpecialAction
{
    public override string Name => "Stack";

    public override uint Phase => 3u;

    public override HashSet<uint> ActionID => new HashSet<uint>();

    public override void OnAddStatus(ActorStatusChangeInfo info)
    {
        if (info.StatusID == 2454)
        {
            SimpleLockon.ShareLockon(info.TargetID.GameObject(), (info.Time - 5f) * 1000f);
        }
    }
}
