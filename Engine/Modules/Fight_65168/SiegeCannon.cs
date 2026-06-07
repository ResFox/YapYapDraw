using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.Fight_65168;

public class SiegeCannon : ISpecialAction
{
    public override string Name => "Siege Cannon";

    public override HashSet<uint> ActionID => new HashSet<uint> { 29020u };

    public override void OnActionCast(ActorCastInfo info)
    {
        SimpleElement.Rectangle(info);
    }
}
