using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.M7S;

public class ElectrogeneticForce : ISpecialAction
{
    public override string Name => "Electrogenetic Force";

    public override uint Phase => 2u;

    public override HashSet<uint> ActionID => new HashSet<uint> { 43340u, 43358u };

    public override void OnActionCast(ActorCastInfo info)
    {
        foreach (IGameObject allPlayer in PlayerHelper.AllPlayers)
        {
            SimpleElement.Circle(allPlayer, 6f, 3000f, 0f, new HitCounter
            {
                ActionID = new HashSet<uint> { 42374u }
            });
        }
    }
}
