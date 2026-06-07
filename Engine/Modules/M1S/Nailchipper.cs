using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.M1S;

public class Nailchipper : ISpecialAction
{
    public override string Name => "Nailchipper";

    public override uint Phase => 3u;

    public override HashSet<uint> ActionID => new HashSet<uint> { 38022u };

    public override void OnActionCast(ActorCastInfo info)
    {
        SimpleElement.Circle(info.TargetId, 5f, 3000f, (info.CastTime - 3f) * 1000f, info.ActionId);
    }
}
