using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.Fight_99991;

public class VisceralWhirl : ISpecialAction
{
    public override string Name => "Visceral Whirl";

    public override HashSet<uint> ActionID => new HashSet<uint> { 35580u, 35581u, 35583u, 35584u };

    public override void OnActionCast(ActorCastInfo info)
    {
        DrawElement drawElement = new DrawElement();
        drawElement.drawAvfx = "general02xf";
        drawElement.Position = info.Pos;
        drawElement.drawOnObject = false;
        drawElement.radiusX = 14f;
        DrawElement drawElement2 = drawElement;
        ushort actionId = info.ActionId;
        bool isShort = actionId == 35580 || actionId == 35583;
        drawElement2.radiusZ = (isShort ? 29 : 60);
        drawElement.refRotation = info.Facing;
        drawElement.fixRotation = true;
        drawElement.hitCounter = new HitCounter
        {
            ActionID = new HashSet<uint> { info.ActionId }
        };
        DrawManager.Draw(drawElement);
    }
}
