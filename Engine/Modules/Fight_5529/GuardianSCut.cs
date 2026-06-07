using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.Fight_5529;

public class GuardianSCut : ISpecialAction
{
    public override string Name => "Guardian's Cut";

    public override HashSet<uint> ActionID => new HashSet<uint> { 26069u };

    public override void OnActionCast(ActorCastInfo info)
    {
        SimpleElement.FanToTarget(info, 40f, 90);
    }
}
