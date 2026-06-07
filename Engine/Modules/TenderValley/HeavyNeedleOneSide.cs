using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.TenderValley;

public class HeavyNeedleOneSide : ISpecialAction
{
    public override string Name => "Heavy Needle (one side)";

    public override HashSet<uint> ActionID => new HashSet<uint> { 39154u, 39155u };

    public override uint Phase => 1u;

    public override void OnActionCast(ActorCastInfo info)
    {
        SimpleElement.Fan(info, 36f, 330);
    }
}
