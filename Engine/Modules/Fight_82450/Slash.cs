using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.ModuleSetup;

namespace YapYapDraw.Modules.Fight_82450;

public class Slash : ISpecialAction
{
    public override string Name => "Slash";

    public override HashSet<uint> ActionID => new HashSet<uint> { 35991u };

    public override uint Phase => 1u;

    public override void OnActorPlayActionTimelineEvent(IGameObject source, uint id)
    {
        if (id == 4566)
        {
            DrawManager.Draw(new DrawElement
            {
                drawAvfx = "general_1bxf",
                radiusX = 9f,
                radiusZ = 9f,
                drawOnObject = true,
                hitCounter = new HitCounter
                {
                    ActionID = new HashSet<uint> { 35991u }
                }
            }, source);
        }
    }
}
