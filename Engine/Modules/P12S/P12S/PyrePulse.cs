using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;
using YapYapDraw.Engine.Util;

namespace YapYapDraw.Modules.P12S.P12S;

public class PyrePulse : ISpecialAction
{
    public override string Name => "Pyre Pulse";

    public override uint Phase => 2u;

    public override HashSet<uint> ActionID => new HashSet<uint>();

    public override void OnAddStatus(ActorStatusChangeInfo info)
    {
        if (info.StatusID == 3590)
        {
            DrawManager.Draw(new DrawElement
            {
                drawAvfx = "general_1bpxf",
                radiusX = 4f,
                radiusZ = 4f,
                drawOnObject = true,
                hitCounter = new HitCounter
                {
                    ActionID = new HashSet<uint> { 33594u }
                }
            }, info.TargetID.GameObject());
        }
    }
}
