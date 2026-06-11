using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.DancingMad.P5;

public class ChaosVortex : ISpecialAction
{
    public override string Name => "Chaos Vortex";

    public override uint Phase => 5u;

    public override HashSet<uint> ActionID => new HashSet<uint> { 47934u };

    public override void OnActionCast(ActorCastInfo info)
    {
        foreach (IGameObject player in PlayerHelper.AllPlayers)
            SimpleLockon.TarLockOn5m5s(player);
    }
}
