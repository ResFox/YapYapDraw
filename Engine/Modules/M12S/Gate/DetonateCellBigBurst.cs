using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;
using YapYapDraw.Engine.Util;

namespace YapYapDraw.Modules.M12S.Gate;

public class DetonateCellBigBurst : ISpecialAction
{
    public override string Name => "Detonate Cell (Big Burst)";

    public override HashSet<uint> ActionID => new HashSet<uint>();

    public override void OnAddStatus(ActorStatusChangeInfo info)
    {
        if (info.StatusID == 4762)
        {
            SimpleLockon.Share6S(info.TargetID.GameObject(), info.Time * 1000f - 6000f);
        }
    }
}
