using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.Fight_56879;

public class WindCrystal : ISpecialAction
{
    public override string Name => "Wind Crystal";

    public override HashSet<uint> ActionID => new HashSet<uint> { 33876u, 33877u, 33878u, 33879u };

    public override uint Phase => 2u;

    public override void OnActionCast(ActorCastInfo info)
    {
        switch (info.ActionId)
        {
        case 33876:
        case 33877:
            SimpleElement.Rectangle(info, 30f, 2.5f);
            break;
        case 33878:
            SimpleElement.Rectangle(info, 30f, 2.5f, 0f, null, 4000f);
            break;
        case 33879:
            SimpleElement.Rectangle(info, 30f, 2.5f, 0f, null, 7500f);
            break;
        }
    }
}
