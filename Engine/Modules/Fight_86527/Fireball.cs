using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.Fight_86527;

public class Fireball : ISpecialAction
{
    public override string Name => "Fireball";

    public override HashSet<uint> ActionID => new HashSet<uint> { 44098u, 44105u };

    public override void OnActionCast(ActorCastInfo info)
    {
        if (info.ActionId == 44105)
        {
            foreach (IGameObject allPlayer in PlayerHelper.AllPlayers)
            {
                SimpleLockon.DrawLockon(allPlayer, "bh_twin_dive_1s");
            }
        }
        SimpleElement.Circle(info);
    }
}
