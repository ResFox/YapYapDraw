using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using FFXIVClientStructs.FFXIV.Client.UI;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.M3S;

public class Diveboom : ISpecialAction
{
    public override string Name => "Diveboom (stack / spread)";

    public override uint Phase => 1u;

    public override HashSet<uint> ActionID => new HashSet<uint> { 37868u, 37869u, 37877u, 37878u };

    public override void OnActionCast(ActorCastInfo info)
    {
        switch (info.ActionId)
        {
        case 37868:
        case 37869:
            SimpleElement.ShowText("Spread soon", (TextGimmickHintStyle)0);
            PlayerHelper.AllPlayers.ForEach(player =>
            {
                SimpleLockon.TarLockOn5m5s(player, 6300f);
            });
            break;
        case 37877:
        case 37878:
            SimpleElement.ShowText("2+2 stacks soon", (TextGimmickHintStyle)0);
            PlayerHelper.Tank.ForEach(player =>
            {
                SimpleLockon.ShareLockon2(player, 6300f);
            });
            PlayerHelper.Healer.ForEach(player =>
            {
                SimpleLockon.ShareLockon2(player, 6300f);
            });
            break;
        }
    }
}
