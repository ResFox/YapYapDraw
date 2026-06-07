using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.ModuleSetup;

namespace YapYapDraw.Modules.Fight_56456;

public class Nightmare : ISpecialAction
{
    public override string Name => "Nightmare";

    public override HashSet<uint> ActionID => new HashSet<uint>();

    public override uint Phase => 1u;

    public override void OnActorPlayActionTimelineEvent(IGameObject source, uint id)
    {
        if (source.BaseId == 16901 && id == 4561)
        {
            DrawManager.Draw(new DrawElement
            {
                drawAvfx = "general_1bxf",
                radiusX = 2f,
                radiusZ = 2f,
                drawOnObject = true,
                hitCounter = new HitCounter
                {
                    ActionID = new HashSet<uint> { 36532u, 36536u }
                }
            }, source);
        }
    }
}
