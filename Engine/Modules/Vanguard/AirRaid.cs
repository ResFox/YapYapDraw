using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;
using YapYapDraw.Engine.Util;

namespace YapYapDraw.Modules.Vanguard;

public class AirRaid : ISpecialAction
{
    public override string Name => "Air Raid";

    public override uint Phase => 1u;

    public override HashSet<uint> ActionID => new HashSet<uint> { 36570u };

    public override void OnActionCast(ActorCastInfo info)
    {
        DrawManager.Draw(new DrawElement
        {
            drawAvfx = "general_1bxf",
            Position = info.Pos,
            drawOnObject = false,
            radiusX = 14f,
            radiusZ = 14f,
            hitCounter = new HitCounter
            {
                ActionID = new HashSet<uint> { 36570u }
            }
        }, info.SourceId.GameObject());
    }
}
