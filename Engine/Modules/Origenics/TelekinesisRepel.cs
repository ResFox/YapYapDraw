using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;
using YapYapDraw.Engine.Util;

namespace YapYapDraw.Modules.Origenics;

public class TelekinesisRepel : ISpecialAction
{
    public override string Name => "Telekinesis Repel";

    public override uint Phase => 3u;

    public override HashSet<uint> ActionID => new HashSet<uint> { 36433u };

    public override void OnActionCast(ActorCastInfo info)
    {
        DrawManager.Draw(new DrawElement
        {
            drawAvfx = "movepos_mark_01t",
            radiusX = 30f,
            radiusY = 5f,
            radiusZ = 30f,
            hitCounter = new HitCounter
            {
                ActionID = new HashSet<uint> { 36433u }
            }
        }, info.SourceId.GameObject());
    }
}
