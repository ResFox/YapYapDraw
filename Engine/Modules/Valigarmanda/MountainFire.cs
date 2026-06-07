using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.Valigarmanda;

public class MountainFire : ISpecialAction
{
    public override string Name => "Mountain Fire";

    public override uint Phase => 1u;

    public override HashSet<uint> ActionID => new HashSet<uint> { 36889u };

    public override void OnActionCast(ActorCastInfo info)
    {
        SimpleElement.Fan(info.SourceId, 40f, 330, info.Facing, 3000f, 0f, 36890u);
    }
}
