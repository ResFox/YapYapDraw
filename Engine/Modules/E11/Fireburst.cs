using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;
using YapYapDraw.Engine.Util;

namespace YapYapDraw.Modules.E11;

public class Fireburst : ISpecialAction
{
    public override string Name => "Fireburst";

    public override HashSet<uint> ActionID => new HashSet<uint> { 22061u, 22084u };

    public override uint Phase => 1u;

    public override void OnActionCast(ActorCastInfo info)
    {
        if (info.ActionId == 22061)
        {
            DrawManager.Draw(new DrawElement
            {
                drawAvfx = "m0792_knockaside_red02k1",
                radiusX = 4f,
                radiusZ = 40f,
                refOffsetZ = 20f,
                drawOnObject = true,
                refRotation = info.Facing,
                fixRotation = true,
                hitCounter = new HitCounter
                {
                    ActionID = new HashSet<uint> { info.ActionId }
                }
            }, info.SourceId.GameObject());
        }
        if (info.ActionId == 22084)
        {
            DrawManager.Draw(new DrawElement
            {
                drawAvfx = "m0792_knockaside_red02k1",
                radiusX = 4f,
                radiusZ = 40f,
                drawOnObject = true,
                refRotation = info.Facing,
                fixRotation = true,
                hitCounter = new HitCounter
                {
                    ActionID = new HashSet<uint> { info.ActionId }
                }
            }, info.SourceId.GameObject());
        }
    }
}
