using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.ClientState.Statuses;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.M4S;

public class StackSpread : ISpecialAction
{
    public override string Name => "Stack / Spread";

    public override uint Phase => 2u;

    public override HashSet<uint> ActionID => new HashSet<uint> { 38380u, 38381u };

    public override void OnActionCast(ActorCastInfo info)
    {
        if (StatusHelper.GetParam(info.SourceId, 2970u, out var param) && param == 752)
        {
            bool found = false;
            foreach (IBattleChara allPlayer in PlayerHelper.AllPlayers)
            {
                IBattleChara player = allPlayer;
                if (player.StatusList.Any((IStatus x) => x.StatusId == 3999))
                {
                    SimpleLockon.ShareLockon2_6m((IGameObject)player, (info.CastTime - 5f) * 1000f);
                    found = true;
                }
            }
            if (found)
            {
                return;
            }
            {
                foreach (IGameObject member in PlayerHelper.Tank.Union(PlayerHelper.Healer))
                {
                    SimpleLockon.ShareLockon2_6m(member, (info.CastTime - 5f) * 1000f);
                }
                return;
            }
        }
        foreach (IGameObject allPlayer2 in PlayerHelper.AllPlayers)
        {
            SimpleLockon.TarLockOn6m5s(allPlayer2, (info.CastTime - 5f) * 1000f);
        }
    }
}
