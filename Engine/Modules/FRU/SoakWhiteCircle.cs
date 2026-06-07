using System.Collections.Generic;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.FRU;

public class SoakWhiteCircle : ISpecialAction
{
    public override string Name => "Soak (white circle)";

    public override uint Phase => 4u;

    public override HashSet<uint> ActionID => new HashSet<uint>();

    public override void OnAddStatus(ActorStatusChangeInfo info)
    {
        _ = info.StatusID;
        _ = 3263;
    }
}
