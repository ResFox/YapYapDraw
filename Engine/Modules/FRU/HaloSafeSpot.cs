using System.Collections.Generic;
using FFXIVClientStructs.FFXIV.Client.UI;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.FRU;

public class HaloSafeSpot : ISpecialAction
{
    public override string Name => "Halo (safe spot)";

    public override uint Phase => 1u;

    public override HashSet<uint> ActionID => new HashSet<uint> { 40150u, 40151u };

    public override void OnActionCast(ActorCastInfo info)
    {
        switch (info.ActionId)
        {
        case 40150:
            SimpleElement.ShowText("Thunder safe", (TextGimmickHintStyle)1);
            break;
        case 40151:
            SimpleElement.ShowText("Fire safe", (TextGimmickHintStyle)1);
            break;
        }
    }
}
