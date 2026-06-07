using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.Fight_99991;

public class DimensionalSurge : ISpecialAction
{
    public override string Name => "Dimensional Surge";

    public override HashSet<uint> ActionID => new HashSet<uint> { 35637u };

    public override uint Phase => 2u;

    public override void OnActionCast(ActorCastInfo info)
    {
        SimpleElement.Rectangle(info, 60f, 7f);
    }
}
