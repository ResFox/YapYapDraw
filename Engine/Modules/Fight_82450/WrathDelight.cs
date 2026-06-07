using System.Collections.Generic;
using FFXIVClientStructs.FFXIV.Client.UI;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.Fight_82450;

public class WrathDelight : ISpecialAction
{
    public override string Name => "Wrath / Delight";

    public override HashSet<uint> ActionID => new HashSet<uint> { 35984u, 35986u, 35989u, 35987u };

    public override uint Phase => 1u;

    public override void OnActionCast(ActorCastInfo info)
    {
        ushort actionId = info.ActionId;
        if (actionId == 35984 || actionId == 35986)
        {
            SimpleElement.ShowText("Go to blue half", (TextGimmickHintStyle)0, 8);
        }
        actionId = info.ActionId;
        if (actionId == 35987 || actionId == 35989)
        {
            SimpleElement.ShowText("Go to red half", (TextGimmickHintStyle)0, 8);
        }
    }
}
