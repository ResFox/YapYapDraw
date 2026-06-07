using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;
using YapYapDraw.Engine.Util;

namespace YapYapDraw.Modules.TEA;

public class CrashingThunder : ISpecialAction
{
    public override string Name => "Crashing Thunder (buff)";

    public override uint Phase => 2u;

    public override HashSet<uint> ActionID => new HashSet<uint>();

    public override void OnAddStatus(ActorStatusChangeInfo info)
    {
        if (info.StatusID == 2143)
        {
            DrawManager.Draw(new DrawElement
            {
                drawAvfx = "general_1bxf",
                radiusX = 8f,
                radiusZ = 8f,
                drawOnObject = true,
                delayDrawTime = (info.Time - 5f) * 1000f,
                hitCounter = new HitCounter
                {
                    ActionID = new HashSet<uint> { 18497u, 18492u }
                }
            }, info.TargetID.GameObject());
        }
    }
}
