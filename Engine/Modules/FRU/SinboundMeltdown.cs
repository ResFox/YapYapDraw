using System;
using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;
using YapYapDraw.Engine.Util;

namespace YapYapDraw.Modules.FRU;

public class SinboundMeltdown : ISpecialAction
{
    public override string Name => "Sinbound Meltdown (hourglass bait)";

    public override uint Phase => 3u;

    public override HashSet<uint> ActionID => new HashSet<uint> { 40291u };

    public override void OnAddStatus(ActorStatusChangeInfo info)
    {
        if (info.StatusID != 2970)
        {
            return;
        }
        WDir wDir = info.TargetID.GameObject().Rotation.Radians().ToDirection();
        DrawElement drawElement = new DrawElement
        {
            drawAvfx = "m0119_trap_02t",
            radiusX = 1.5f,
            radiusY = 5f,
            radiusZ = 1.5f,
            drawOnObject = true,
            refOffsetX = ((info.Stack == 269) ? (-2f * Math.Abs(wDir.X)) : (2f * Math.Abs(wDir.X))),
            hitCounter = new HitCounter
            {
                ActionID = new HashSet<uint> { 40235u }
            }
        };
        if (Math.Abs(info.TargetID.GameObject().Rotation.RadiansToDegrees() % 90f) <= 1f)
        {
            switch (info.Stack)
            {
            case 269u:
                drawElement.refOffsetX = -2f;
                break;
            case 348u:
                drawElement.refOffsetX = 2f;
                break;
            }
        }
        DrawManager.Draw(drawElement, info.TargetID.GameObject());
    }
}
