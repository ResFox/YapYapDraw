using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;
using YapYapDraw.Engine.Util;

namespace YapYapDraw.Modules.M6S;

public class HeatingBurningUp : ISpecialAction
{
    public override string Name => "Heating/Burning Up";

    public override uint Phase => 2u;

    public override HashSet<uint> ActionID => new HashSet<uint>();

    public override void OnAddStatus(ActorStatusChangeInfo info)
    {
        if (info.StatusID == 4454)
        {
            SimpleElement.Circle(info.TargetID, 15f, 5000f, (info.Time - 5f) * 1000f);
        }
        if (info.StatusID == 4448)
        {
            SimpleLockon.ShareLockon(info.TargetID.GameObject(), (info.Time - 5f) * 1000f);
        }
    }
}
