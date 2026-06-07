using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;
using YapYapDraw.Engine.Util;

namespace YapYapDraw.Modules.UnendingCoil;

public class TenstrikeTrio : ISpecialAction
{
    public override string Name => "Tenstrike Trio";

    public override uint Phase => 5u;

    public override HashSet<uint> ActionID => new HashSet<uint> { 9964u };

    public override void OnActionCast(ActorCastInfo info)
    {
        DrawManager.Draw(new DrawElement
        {
            drawAvfx = "general_1bpxf",
            radiusX = 4f,
            radiusZ = 4f,
            hitCounter = new HitCounter
            {
                ActionID = new HashSet<uint> { info.ActionId }
            }
        }, info.TargetId.GameObject());
        SimpleLockon.Share6S(info.TargetId.GameObject());
    }
}
