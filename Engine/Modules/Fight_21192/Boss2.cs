using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.Fight_21192;

public class Boss2 : ISpecialAction
{
    public override string Name => "Necrotic Crisis";

    public override HashSet<uint> ActionID => new HashSet<uint> { 40646u };

    public override uint Phase => 2u;

    public override void OnActionCast(ActorCastInfo info)
    {
        SimpleElement.Circle(info, 18f);
    }
}
