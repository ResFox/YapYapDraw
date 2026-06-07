using System.Collections.Generic;
using System.Numerics;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;
using YapYapDraw.Engine.Util;

namespace YapYapDraw.Modules.Vanguard;

public class Charge : ISpecialAction
{
    public override string Name => "Charge";

    public override uint Phase => 1u;

    public override HashSet<uint> ActionID => new HashSet<uint> { 36569u };

    public override void OnActionCast(ActorCastInfo info)
    {
        DrawManager.Draw(new DrawElement
        {
            drawAvfx = "general02xf",
            radiusX = 2.5f,
            drawOnObject = true,
            targetPosition = new Vector3(info.Pos.X, info.Pos.Y, info.Pos.Z),
            endToTarget = true,
            hitCounter = new HitCounter
            {
                ActionID = new HashSet<uint> { 36569u }
            }
        }, info.SourceId.GameObject());
    }
}
