using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;
using YapYapDraw.Engine.Util;

namespace YapYapDraw.Modules.M8S;

public class Towerfall : ISpecialAction
{
    public override string Name => "Towerfall";

    public override HashSet<uint> ActionID => new HashSet<uint> { 41925u };

    public override void OnActionCast(ActorCastInfo info)
    {
        DrawElement element = new DrawElement
        {
            drawAvfx = "general02xf",
            Position = info.Pos,
            drawOnObject = false,
            radiusX = 5f,
            radiusZ = 30f,
            refRotation = info.Facing,
            fixRotation = true,
            hitCounter = new HitCounter
            {
                ActionID = new HashSet<uint> { 41926u }
            }
        };
        DrawManager.Draw(element);
        element.refRotation += 180.Degrees();
        DrawManager.Draw(element);
    }
}
