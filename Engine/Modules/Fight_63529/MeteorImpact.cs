using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.Fight_63529;

public class MeteorImpact : ISpecialAction
{
    public override string Name => "Meteor Impact";

    public override uint Phase => 1u;

    public override HashSet<uint> ActionID => new HashSet<uint> { 35676u };

    public override void OnActionCast(ActorCastInfo info)
    {
        SimpleElement.Circle(info, 10f);
    }
}
