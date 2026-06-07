using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.Fight_64923;

public class BlazingClaw : ISpecialAction
{
    public override string Name => "Blazing Claw";

    public override HashSet<uint> ActionID => new HashSet<uint> { 31967u, 31968u, 31969u };

    public override uint Phase => 1u;

    public override void OnActionCast(ActorCastInfo info)
    {
        switch (info.ActionId)
        {
        case 31967:
            SimpleElement.Rectangle(info, 40f, 6f, 40f);
            break;
        case 31968:
            SimpleElement.Rectangle(info, 8f, 20f, 0f, null, 1800f);
            break;
        case 31969:
            SimpleElement.Rectangle(info, 8f, 20f, 0f, null, 2800f);
            break;
        }
    }
}
