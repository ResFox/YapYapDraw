using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;
using YapYapDraw.Engine.Util;

namespace YapYapDraw.Modules.A4S;

public class Eschatos : ISpecialAction
{
    public override string Name => "Eschatos";

    public override HashSet<uint> ActionID => new HashSet<uint> { 5969u };

    public override void OnActionCast(ActorCastInfo info)
    {
        DrawManager.Draw(new DrawElement
        {
            drawAvfx = "gl_fan090_1bf",
            radiusX = 25f,
            radiusZ = 25f,
            drawOnObject = true,
            refRotation = info.Facing,
            fixRotation = true,
            hitCounter = new HitCounter
            {
                ActionID = new HashSet<uint> { 5970u },
                TargetHitCount = 5
            }
        }, info.SourceId.GameObject());
    }
}
