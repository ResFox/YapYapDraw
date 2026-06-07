using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.M12S.Gate;

public class RavenousReach : ISpecialAction
{
    public override string Name => "Ravenous Reach";

    public override HashSet<uint> ActionID => new HashSet<uint> { 46237u };

    public override void OnActionCast(ActorCastInfo info)
    {
        SimpleElement.Fan(info);
    }
}
