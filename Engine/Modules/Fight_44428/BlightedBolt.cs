using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;
using YapYapDraw.Engine.Util;

namespace YapYapDraw.Modules.Fight_44428;

public class BlightedBolt : ISpecialAction
{
    public override string Name => "Blighted Bolt";

    public override HashSet<uint> ActionID => new HashSet<uint> { 36174u };

    public override void OnActionCast(ActorCastInfo info)
    {
        DrawManager.Draw(new DrawElement
        {
            drawAvfx = "general_1bxf",
            radiusX = 7f,
            radiusZ = 7f,
            drawOnObject = false,
            hitCounter = new HitCounter
            {
                ActionID = new HashSet<uint> { info.ActionId }
            }
        }, info.TargetId.GameObject());
    }
}
