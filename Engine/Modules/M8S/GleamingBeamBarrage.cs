using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.M8S;

public class GleamingBeamBarrage : ISpecialAction
{
    public override string Name => "Gleaming Beam / Barrage";

    public override uint Phase => 2u;

    public override HashSet<uint> ActionID => new HashSet<uint> { 42078u, 42102u };

    public override void OnActionCast(ActorCastInfo info)
    {
        SimpleElement.Rectangle(info);
    }
}
