using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.JeunoArc1;

public class Gigastrike : ISpecialAction
{
    public override string Name => "Gigastrike";

    public override HashSet<uint> ActionID => new HashSet<uint> { 40774u, 40775u, 40776u, 40777u };

    public override uint Phase => 4u;

    public override void OnActionCast(ActorCastInfo info)
    {
        switch (info.ActionId)
        {
        case 40774:
        case 40776:
            SimpleElement.Fan(info.SourceId, 90f, 180, info.Facing, info.CastTime * 1000f, 0f, 0u);
            break;
        case 40775:
        case 40777:
            SimpleElement.Fan(info.SourceId, 12f, 180, info.Facing, info.CastTime * 1000f, 0f, 0u);
            break;
        }
    }
}
