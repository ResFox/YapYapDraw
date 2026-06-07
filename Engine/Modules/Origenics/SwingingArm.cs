using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.Origenics;

public class SwingingArm : ISpecialAction
{
    public override string Name => "Swinging Arm";

    public override uint Phase => 2u;

    public override HashSet<uint> ActionID => new HashSet<uint> { 36370u };

    public override void OnActionCast(ActorCastInfo info)
    {
        SimpleElement.Fan(info, 30f, 90);
    }
}
