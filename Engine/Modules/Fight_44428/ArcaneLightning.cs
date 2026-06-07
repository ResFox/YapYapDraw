using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Util;

namespace YapYapDraw.Modules.Fight_44428;

public class ArcaneLightning : ISpecialAction
{
    public override string Name => "Arcane Lightning";

    public override HashSet<uint> ActionID => new HashSet<uint>();

    public override void OnObjectCreatedEvent(IGameObject GameObject)
    {
        if (GameObject.BaseId == 16769)
        {
            DrawManager.Draw(new DrawElement
            {
                drawAvfx = "general02wf",
                radiusX = 2.5f,
                radiusZ = 50f,
                drawOnObject = true,
                refRotation = GameObject.Rotation.Radians(),
                fixRotation = true,
                hitCounter = new HitCounter
                {
                    ActionID = new HashSet<uint> { 39001u }
                }
            }, GameObject);
        }
    }
}
