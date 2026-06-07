using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;
using YapYapDraw.Engine.Util;

namespace YapYapDraw.Modules.M12S.Gate;

public class DetonateCell : ISpecialAction
{
    public override string Name => "Detonate Cell";

    public override HashSet<uint> ActionID => new HashSet<uint>();

    public override void OnAddStatus(ActorStatusChangeInfo info)
    {
        if (info.StatusID == 4761)
        {
            SimpleLockon.TarLockOn6m5s(info.TargetID.GameObject(), info.Time * 1000f - 5000f);
        }
    }
}
