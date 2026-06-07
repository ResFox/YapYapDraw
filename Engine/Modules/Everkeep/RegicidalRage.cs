using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.Everkeep;

public class RegicidalRage : ISpecialAction
{
    public override string Name => "Regicidal Rage";

    public override uint Phase => 1u;

    public override HashSet<uint> ActionID => new HashSet<uint> { 39227u };

    public override void OnActionCast(ActorCastInfo info)
    {
        foreach (IGameObject allPlayer in PlayerHelper.AllPlayers)
        {
            DrawManager.Draw(new DrawElement
            {
                drawAvfx = "general_1bxf",
                radiusX = 8f,
                radiusZ = 8f,
                drawOnObject = true,
                TetherCheck = new TetherCheck
                {
                    CheckType = 0,
                    TetherID = new HashSet<int> { 89 }
                },
                hitCounter = new HitCounter
                {
                    ActionID = new HashSet<uint> { 39228u }
                }
            }, allPlayer);
        }
    }
}
