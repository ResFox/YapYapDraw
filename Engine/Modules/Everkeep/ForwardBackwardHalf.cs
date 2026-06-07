using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using FFXIVClientStructs.FFXIV.Client.UI;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;
using YapYapDraw.Engine.Util;

namespace YapYapDraw.Modules.Everkeep;

public class ForwardBackwardHalf : ISpecialAction
{
    public override string Name => "Forward / Backward Half";

    public override uint Phase => 1u;

    public override HashSet<uint> ActionID => new HashSet<uint> { 37755u, 37756u, 37757u, 37758u, 39322u, 39323u, 39324u, 39325u };

    public override void OnActionCast(ActorCastInfo info)
    {
        (bool, bool) halves;
        switch (info.ActionId)
        {
        case 37755:
        case 39322:
            halves = (true, false);
            break;
        case 37756:
        case 39323:
            halves = (true, true);
            break;
        case 37757:
        case 39324:
            halves = (false, false);
            break;
        case 37758:
        case 39325:
            halves = (false, true);
            break;
        default:
            halves = default;
            break;
        }
        var (backHalf, rightSide) = halves;
        switch (info.ActionId)
        {
        case 37755:
        case 39322:
            SimpleElement.ShowText("Front-right safe", (TextGimmickHintStyle)0);
            break;
        case 37756:
        case 39323:
            SimpleElement.ShowText("Front-left safe", (TextGimmickHintStyle)0);
            break;
        case 37757:
        case 39324:
            SimpleElement.ShowText("Back-left safe", (TextGimmickHintStyle)0);
            break;
        case 37758:
        case 39325:
            SimpleElement.ShowText("Back-right safe", (TextGimmickHintStyle)0);
            break;
        }
        Angle angle = info.Facing + (backHalf ? 180 : 0).Degrees();
        IGameObject? source = info.SourceId.GameObject();
        Angle rotation = angle;
        HitCounter hitCounter = new HitCounter
        {
            ActionID = new HashSet<uint> { 37759u, 39282u, 37760u }
        };
        SimpleElement.Rectangle(source, 50f, 30f, 10f, null, rotation, 3000f, 0f, hitCounter);
        IGameObject? source2 = info.SourceId.GameObject();
        rotation = angle + (rightSide ? 90 : (-90)).Degrees();
        hitCounter = new HitCounter
        {
            ActionID = new HashSet<uint> { 37759u, 39282u, 37760u }
        };
        SimpleElement.Rectangle(source2, 60f, 60f, 0f, null, rotation, 3000f, 0f, hitCounter);
    }
}
