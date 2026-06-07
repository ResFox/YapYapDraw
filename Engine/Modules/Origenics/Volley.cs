using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;
using YapYapDraw.Engine.Util;

namespace YapYapDraw.Modules.Origenics;

public class Volley : ISpecialAction
{
    public override string Name => "Volley";

    public override uint Phase => 2u;

    public override HashSet<uint> ActionID => new HashSet<uint> { 36372u };

    public override void OnActionCast(ActorCastInfo info)
    {
        DrawManager.Draw(new DrawElement
        {
            drawAvfx = "m0248_freeze_o0c",
            radiusX = 2f,
            radiusZ = 40f,
            drawOnObject = true,
            refRotation = info.Facing,
            fixRotation = true,
            hitCounter = new HitCounter
            {
                ActionID = new HashSet<uint> { 36372u }
            }
        }, info.SourceId.GameObject());
    }
}
