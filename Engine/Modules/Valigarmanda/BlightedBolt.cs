using System.Collections.Generic;
using FFXIVClientStructs.FFXIV.Client.UI;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.Valigarmanda;

public class BlightedBolt : ISpecialAction
{
    public override string Name => "Blighted Bolt";

    public override uint Phase => 2u;

    public override HashSet<uint> ActionID => new HashSet<uint> { 36831u, 36833u };

    public override void OnActionCast(ActorCastInfo info)
    {
        if (info.ActionId == 36831)
        {
            SimpleElement.ShowText("Don't float", (TextGimmickHintStyle)1);
        }
        if (info.ActionId == 36833)
        {
            SimpleElement.Circle(info.TargetId, 8f, 3000f, 0f, 36833u);
        }
    }
}
