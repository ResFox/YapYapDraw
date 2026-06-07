using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;
using YapYapDraw.Engine.Util;

namespace YapYapDraw.Modules.M4S;

public class WickedJolt : ISpecialAction
{
    public override string Name => "Wicked Jolt";

    public override uint Phase => 2u;

    public override HashSet<uint> ActionID => new HashSet<uint> { 38384u };

    public override void OnActionCast(ActorCastInfo info)
    {
        DrawManager.Draw(new DrawElement
        {
            drawAvfx = "general02xf",
            radiusX = 2.5f,
            radiusZ = 60f,
            drawOnObject = true,
            target = info.TargetId.GameObject(),
            alwaysFaceCurrentTarget = true,
            hitCounter = new HitCounter
            {
                ActionID = new HashSet<uint> { 38384u, 38385u },
                TargetHitCount = 2
            }
        }, info.SourceId.GameObject());
    }
}
