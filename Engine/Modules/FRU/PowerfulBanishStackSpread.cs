using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.ClientState.Objects.Types;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.FRU;

public class PowerfulBanishStackSpread : ISpecialAction
{
    public override string Name => "Powerful Banish (stack / spread)";

    public override uint Phase => 2u;

    public override HashSet<uint> ActionID => new HashSet<uint> { 40220u, 40221u };

    public override void OnActionCast(ActorCastInfo info)
    {
        switch (info.ActionId)
        {
        case 40220:
        {
            foreach (IGameObject player in PlayerHelper.Tank.Union(PlayerHelper.Healer))
            {
                SimpleLockon.ShareLockon2(player);
            }
            break;
        }
        case 40221:
        {
            foreach (IGameObject player in PlayerHelper.AllPlayers)
            {
                SimpleLockon.TarLockOn5m5s(player);
            }
            break;
        }
        }
    }
}
