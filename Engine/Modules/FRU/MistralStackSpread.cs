using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using FFXIVClientStructs.FFXIV.Client.UI;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.FRU;

public class MistralStackSpread : ISpecialAction
{
    public override string Name => "Mistral (stack / spread)";

    public override uint Phase => 1u;

    public override HashSet<uint> ActionID => new HashSet<uint> { 40155u, 40154u };

    public override void OnActionCast(ActorCastInfo info)
    {
        if (info.ActionId == 40155)
        {
            SimpleElement.ShowText("Spread soon", (TextGimmickHintStyle)0);
            {
                foreach (IGameObject player in PlayerHelper.AllPlayers)
                {
                    SimpleLockon.TarLockOn5m5s(player, 14000f);
                }
                return;
            }
        }
        if (info.ActionId != 40154)
        {
            return;
        }
        SimpleElement.ShowText("Stack soon", (TextGimmickHintStyle)0);
        foreach (IGameObject healer in PlayerHelper.Healer)
        {
            SimpleLockon.ShareLockon(healer, 14000f);
        }
    }
}
