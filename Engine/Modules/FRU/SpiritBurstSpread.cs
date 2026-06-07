using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.FRU;

public class SpiritBurstSpread : ISpecialAction
{
    public override string Name => "Spirit Burst (spread)";

    public override uint Phase => 3u;

    public override HashSet<uint> ActionID => new HashSet<uint> { 40288u };

    public override void OnActionCast(ActorCastInfo info)
    {
        foreach (IGameObject allPlayer in PlayerHelper.AllPlayers)
        {
            SimpleElement.Circle(allPlayer, 5f, 3000f, 0f, new HitCounter
            {
                ActionID = new HashSet<uint> { 40289u }
            });
        }
    }
}
