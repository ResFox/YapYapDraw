using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.M3S;

public class Selfdestruct : ISpecialAction
{
    public override string Name => "Fuses of Fury (self-destruct)";

    public override uint Phase => 2u;

    public override HashSet<uint> ActionID => new HashSet<uint>();

    public override void OnAddStatus(ActorStatusChangeInfo info)
    {
        switch (info.StatusID)
        {
        case 4017u:
            SimpleElement.Circle(info.TargetID, 8f, 5000f);
            break;
        case 4018u:
            SimpleElement.Circle(info.TargetID, 8f, 5000f, 5000f);
            break;
        }
    }
}
