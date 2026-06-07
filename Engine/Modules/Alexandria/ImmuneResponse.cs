using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.Alexandria;

public class ImmuneResponse : ISpecialAction
{
    public override string Name => "Immune Response";

    public override uint Phase => 1u;

    public override HashSet<uint> ActionID => new HashSet<uint> { 36379u, 36381u };

    public override void OnActionCast(ActorCastInfo info)
    {
        switch (info.ActionId)
        {
        case 36379:
            SimpleElement.Fan(info, 40f, 120);
            break;
        case 36381:
            SimpleElement.Fan(info, 40f, 240);
            break;
        }
    }
}
