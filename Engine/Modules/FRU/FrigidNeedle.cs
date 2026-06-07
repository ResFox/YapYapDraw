using System.Collections.Generic;
using System.Numerics;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;
using YapYapDraw.Engine.Util;

namespace YapYapDraw.Modules.FRU;

public class FrigidNeedle : ISpecialAction
{
    public override string Name => "Frigid Needle";

    public override uint Phase => 2u;

    public override HashSet<uint> ActionID => new HashSet<uint> { 40201u };

    public override void OnActionCast(ActorCastInfo info)
    {
        DrawElement rect = new DrawElement
        {
            drawAvfx = "customRect2",
            radiusX = 2.5f,
            radiusZ = 40f,
            drawOnObject = true,
            refRotation = info.Facing,
            fixRotation = true,
            hitCounter = new HitCounter
            {
                ActionID = ActionID
            },
            refColor = new Vector4(1f, 1f, 1f, 0.2f),
            refTargetColor = new Vector4(1f, 1f, 1f, 0.2f)
        };
        DrawManager.Draw(rect, info.SourceId.GameObject());
        rect.refRotation += 90.Degrees();
        DrawManager.Draw(rect, info.SourceId.GameObject());
    }
}
