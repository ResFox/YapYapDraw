using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;
using YapYapDraw.Engine.Util;

namespace YapYapDraw.Modules.DancingMad.P5;

public class MeteorStack : ISpecialAction
{
    public override string Name => "Meteor Stack";

    public override uint Phase => 5u;

    public override HashSet<uint> ActionID => new HashSet<uint>();

    public override void OnAddStatus(ActorStatusChangeInfo info)
    {
        if (info.StatusID != 5350)
            return;

        DrawManager.Draw(new DrawElement
        {
            drawAvfx = "general_1bxf",
            radiusX = 25f,
            radiusZ = 25f,
            hitCounter = new HitCounter
            {
                ActionID = new HashSet<uint> { 47957u }
            }
        }, info.TargetID.GameObject());
    }
}
