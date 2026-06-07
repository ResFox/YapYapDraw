using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;
using YapYapDraw.Engine.Util;

namespace YapYapDraw.Modules.M4S;

public class MustardBombDebuff : ISpecialAction
{
    public override string Name => "Mustard Bomb (debuff)";

    public override uint Phase => 5u;

    public override HashSet<uint> ActionID => new HashSet<uint>();

    public override void OnAddStatus(ActorStatusChangeInfo info)
    {
        if (info.StatusID == 4007)
        {
            DrawManager.Draw(new DrawElement
            {
                drawAvfx = "general_1bxf",
                radiusX = 6f,
                radiusZ = 6f,
                drawOnObject = true,
                StatusCheck = new StatusCheck
                {
                    CheckObject = info.TargetID.GameObject(),
                    Status = 4007u
                },
                hitCounter = new HitCounter
                {
                    ActionID = new HashSet<uint> { 38433u }
                }
            }, info.TargetID.GameObject());
        }
    }
}
