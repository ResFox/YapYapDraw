using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Util;

namespace YapYapDraw.Modules.DSR;

public class SpiralPierce : ISpecialAction
{
    public override string Name => "Spiral Pierce";

    public override uint Phase => 5u;

    public override uint WeatherID => 46u;

    public override HashSet<uint> ActionID => new HashSet<uint> { 0u };

    public override void OnActorTetherEvent(uint actorId, uint Id, ulong targetId)
    {
        if (Id == 5)
        {
            IGameObject source = actorId.GameObject();
            IGameObject target = targetId.GameObject();
            if (source != null && target != null)
            {
                DrawManager.Draw(new DrawElement
                {
                    drawAvfx = "general02xf",
                    radiusX = 8f,
                    radiusZ = 60f,
                    drawOnObject = true,
                    endToTarget = true,
                    target = target,
                    TetherCheck = new TetherCheck
                    {
                        CheckType = 1,
                        TetherID = new HashSet<int> { 5 }
                    },
                    hitCounter = new HitCounter
                    {
                        ActionID = new HashSet<uint> { 27530u }
                    }
                }, source);
            }
        }
    }
}
