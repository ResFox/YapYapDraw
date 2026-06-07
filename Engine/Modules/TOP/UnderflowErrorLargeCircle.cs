using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;
using YapYapDraw.Engine.Util;

namespace YapYapDraw.Modules.TOP;

public class UnderflowErrorLargeCircle : ISpecialAction
{
    public override string Name => "Underflow Error (large circle)";

    public override uint Phase => 3u;

    public override HashSet<uint> ActionID => new HashSet<uint>();

    public override void OnAddStatus(ActorStatusChangeInfo info)
    {
        if (info.StatusID == 3525)
        {
            DrawManager.Draw(new DrawElement
            {
                drawAvfx = "general_1bxf",
                radiusX = 20f,
                radiusZ = 20f,
                drawOnObject = true,
                delayDrawTime = (info.Time - 6f) * 1000f,
                hitCounter = new HitCounter
                {
                    ActionID = new HashSet<uint> { 31575u },
                    HitTarget = info.TargetID.GameObject()
                }
            }, info.TargetID.GameObject());
        }
    }
}
