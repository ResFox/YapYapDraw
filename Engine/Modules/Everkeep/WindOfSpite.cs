using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;
using YapYapDraw.Engine.Util;

namespace YapYapDraw.Modules.Everkeep;

public class WindOfSpite : ISpecialAction
{
    public override string Name => "Wind of Spite";

    public override uint Phase => 3u;

    public override HashSet<uint> ActionID => new HashSet<uint> { 39229u };

    public override void OnActionCast(ActorCastInfo info)
    {
        DrawManager.Draw(new DrawElement
        {
            drawAvfx = "general_1bxf",
            radiusX = 5f,
            radiusZ = 5f,
            drawOnObject = true,
            alwaysDrawOnCurrentTarget = true,
            hitCounter = new HitCounter
            {
                ActionID = new HashSet<uint> { 39230u, 39232u },
                TargetHitCount = 3
            }
        }, info.SourceId.GameObject());
    }
}
