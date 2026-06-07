using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;
using YapYapDraw.Engine.Util;

namespace YapYapDraw.Modules.Fight_7396;

public class Charge : ISpecialAction
{
    public override string Name => "Charge";

    public override HashSet<uint> ActionID => new HashSet<uint> { 10349u, 10360u };

    public override void OnActionCast(ActorCastInfo info)
    {
        DrawManager.Draw(new DrawElement
        {
            drawAvfx = "general02xf",
            Position = info.SourceId.GameObject().Position,
            drawOnObject = false,
            radiusX = 5f,
            targetPosition = info.Pos,
            endToTarget = true,
            hitCounter = new HitCounter
            {
                ActionID = new HashSet<uint> { info.ActionId }
            }
        });
    }
}
