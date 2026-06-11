using System.Collections.Generic;
using System.Numerics;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;
using YapYapDraw.Engine.Util;

namespace YapYapDraw.Modules.DancingMad.P5;

public class ChaosApocalypse : ISpecialAction
{
    public override string Name => "Chaos Apocalypse";

    public override uint Phase => 5u;

    public override HashSet<uint> ActionID => new HashSet<uint> { 47932u };

    public override void OnActionCast(ActorCastInfo info)
    {
        WDir dir = info.Facing.ToDirection();
        WPos origin = new WPos(info.Pos.X, info.Pos.Z);
        for (int i = 0; i < 8; i++)
        {
            WPos pos = origin + 7f * (float)i * dir;
            DrawManager.Draw(new DrawElement
            {
                drawAvfx = "m0347_sircle_01m1",
                Position = new Vector3(pos.X, 0f, pos.Z),
                drawOnObject = false,
                radiusX = 6f,
                radiusZ = 6f,
                destroyTime = 4500 + i * 500
            });
        }
    }
}
