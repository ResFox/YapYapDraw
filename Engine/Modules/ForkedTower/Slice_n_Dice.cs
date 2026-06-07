using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.ForkedTower;

public class Slice_n_Dice : ISpecialAction
{
    public override string Name => "Slice 'n' Dice";

    public override HashSet<uint> ActionID => new HashSet<uint> { 42498u };

    public override uint Phase => 2u;

    public override void OnActionCast(ActorCastInfo info)
    {
        SimpleElement.FanToTarget(info, 70f, 90);
    }
}
