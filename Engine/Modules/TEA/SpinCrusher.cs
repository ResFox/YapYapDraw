using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.TEA;

public class SpinCrusher : ISpecialAction
{
    public override string Name => "Spin Crusher";

    public override uint Phase => 2u;

    public override HashSet<uint> ActionID => new HashSet<uint> { 19058u };

    public override void OnActionCast(ActorCastInfo info)
    {
        SimpleElement.Fan(info, 10f, 90);
    }
}
