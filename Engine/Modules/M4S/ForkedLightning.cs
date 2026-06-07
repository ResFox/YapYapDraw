using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;
using YapYapDraw.Engine.Util;

namespace YapYapDraw.Modules.M4S;

public class ForkedLightning : ISpecialAction
{
    public override string Name => "Forked Lightning (buff)";

    public override uint Phase => 1u;

    public override HashSet<uint> ActionID => new HashSet<uint>();

    public override void OnAddStatus(ActorStatusChangeInfo info)
    {
        if (info.StatusID == 587)
        {
            SimpleLockon.TarLockOn5m8s(info.TargetID.GameObject());
        }
    }
}
