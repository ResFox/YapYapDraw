using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.Fight_44428;

public class SlitheringStrike : ISpecialAction
{
    public override string Name => "Slithering Strike";

    public override HashSet<uint> ActionID => new HashSet<uint> { 36158u };

    public override void OnActionCast(ActorCastInfo info)
    {
        SimpleElement.Fan(info);
    }
}
