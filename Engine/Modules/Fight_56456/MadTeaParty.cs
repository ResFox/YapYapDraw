using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;
using YapYapDraw.Engine.Util;

namespace YapYapDraw.Modules.Fight_56456;

public class MadTeaParty : ISpecialAction
{
    public override string Name => "Mad Tea Party";

    public override HashSet<uint> ActionID => new HashSet<uint>();

    public override uint Phase => 2u;

    public override void OnAddStatus(ActorStatusChangeInfo info)
    {
        if (info.StatusID == 1909 && info.Stack == 1)
        {
            DrawManager.Draw(new DrawElement
            {
                drawAvfx = "general_1bxf",
                radiusX = 8f,
                radiusZ = 8f,
                drawOnObject = true,
                destroyTime = 9000f
            }, info.TargetID.GameObject());
        }
    }
}
