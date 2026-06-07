using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.DSR;

public class FullDimension : ISpecialAction
{
    public override string Name => "Full Dimension";

    public override uint Phase => 1u;

    public override HashSet<uint> ActionID => new HashSet<uint> { 25307u };

    public override void OnActionCast(ActorCastInfo info)
    {
        SimpleElement.Circle(info, 6f);
    }
}
