using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.ModuleSetup;

namespace YapYapDraw.Modules.Praetorium;

public class GraceTermination : ISpecialAction
{
    public override string Name => "Grace Termination";

    public override HashSet<uint> ActionID => new HashSet<uint>();

    public override uint Phase => 3u;

    public override void OnObjectCreatedEvent(IGameObject GameObject)
    {
        if (GameObject.BaseId == 14456)
        {
            DrawManager.Draw(new DrawElement
            {
                drawAvfx = "general02xf",
                radiusX = 2f,
                radiusZ = 40f,
                drawOnObject = true,
                hitCounter = new HitCounter
                {
                    ActionID = new HashSet<uint> { 28487u, 28488u }
                }
            }, GameObject);
        }
    }
}
