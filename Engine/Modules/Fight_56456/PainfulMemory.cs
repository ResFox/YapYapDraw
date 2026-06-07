using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.Fight_56456;

public class PainfulMemory : ISpecialAction
{
    public override string Name => "Painful Memory";

    public override HashSet<uint> ActionID => new HashSet<uint> { 37139u, 37147u };

    public override uint Phase => 3u;

    public override void OnActionCast(ActorCastInfo info)
    {
        switch (info.ActionId)
        {
        case 37139:
            SimpleElement.Rectangle(info, 50f, 8f);
            break;
        case 37147:
            SimpleElement.Rectangle(info, 50f, 6f);
            break;
        }
    }
}
