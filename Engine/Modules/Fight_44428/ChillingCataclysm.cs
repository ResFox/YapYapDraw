using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Util;

namespace YapYapDraw.Modules.Fight_44428;

public class ChillingCataclysm : ISpecialAction
{
    public override string Name => "Chilling Cataclysm";

    public override HashSet<uint> ActionID => new HashSet<uint>();

    public override void OnObjectCreatedEvent(IGameObject GameObject)
    {
        if (GameObject.BaseId == 17555)
        {
            DrawElement element = new DrawElement
            {
                drawAvfx = "general_x02f",
                radiusX = 2.5f,
                radiusZ = 40f,
                drawOnObject = true,
                fixRotation = true,
                hitCounter = new HitCounter
                {
                    ActionID = new HashSet<uint> { 39265u }
                }
            };
            DrawManager.Draw(element, GameObject);
            element.refRotation = 45.Degrees();
            DrawManager.Draw(element, GameObject);
            element.refRotation = 90.Degrees();
            DrawManager.Draw(element, GameObject);
            element.refRotation = 135.Degrees();
            DrawManager.Draw(element, GameObject);
        }
    }
}
