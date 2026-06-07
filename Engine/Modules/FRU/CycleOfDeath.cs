using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.FRU;

public class CycleOfDeath : ISpecialAction
{
    public override string Name => "Cycle of Death";

    public override uint Phase => 5u;

    public override HashSet<uint> ActionID => new HashSet<uint> { 40310u };

    public override void OnActionCast(ActorCastInfo info)
    {
        foreach (IGameObject tank in PlayerHelper.Tank)
        {
            SimpleLockon.ShareLockon(tank, 3000f);
        }
    }
}
