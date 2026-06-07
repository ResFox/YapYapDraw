using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;
using YapYapDraw.Engine.Util;

namespace YapYapDraw.Modules.FRU;

public class FireStack : ISpecialAction
{
    public override string Name => "Fire (stack)";

    public override uint Phase => 1u;

    public override HashSet<uint> ActionID => new HashSet<uint>();

    public override void OnAddStatus(ActorStatusChangeInfo info)
    {
        if (info.StatusID == 4165)
        {
            SimpleLockon.ShareLockon(info.TargetID.GameObject(), 3000f);
            DrawManager.Draw(new DrawElement
            {
                drawAvfx = "general_1bpxf",
                radiusX = 6f,
                radiusZ = 6f,
                drawOnObject = true,
                delayDrawTime = (info.Time - 5f) * 1000f,
                hitCounter = new HitCounter
                {
                    ActionID = new HashSet<uint> { 40167u }
                }
            }, info.TargetID.GameObject());
        }
    }
}
