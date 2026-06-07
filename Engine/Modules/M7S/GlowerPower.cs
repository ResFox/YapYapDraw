using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.M7S;

public class GlowerPower : ISpecialAction
{
    public override string Name => "Glower Power";

    public override uint Phase => 2u;

    public override HashSet<uint> ActionID => new HashSet<uint> { 43340u, 43358u };

    public override void OnActionCast(ActorCastInfo info)
    {
        SimpleElement.Rectangle(info);
    }
}
