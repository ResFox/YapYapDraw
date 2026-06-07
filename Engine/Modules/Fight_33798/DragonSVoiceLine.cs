using System.Collections.Generic;
using System.Numerics;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;
using YapYapDraw.Engine.Util;

namespace YapYapDraw.Modules.Fight_33798;

public class DragonSVoiceLine : ISpecialAction
{
    public override string Name => "Dragon's Voice (line)";

    public override HashSet<uint> ActionID => new HashSet<uint> { 43877u };

    public override void OnActionCast(ActorCastInfo info)
    {
        WDir wDir = info.Facing.ToDirection();
        for (int i = 0; i < 5; i++)
        {
            WPos wPos = new WPos(info.Pos) + 8 * i * wDir;
            DrawManager.Draw(new DrawElement
            {
                drawAvfx = "general02xf",
                Position = new Vector3(wPos.X, 0f, wPos.Z),
                drawOnObject = false,
                radiusX = 20f,
                radiusZ = 8f,
                refRotation = info.Facing,
                fixRotation = true,
                delayDrawTime = ((i == 0) ? 0f : (info.CastTime * 1000f + (float)((i - 1) * 2500))),
                destroyTime = ((i == 0) ? (info.CastTime * 1000f) : 2500f)
            });
        }
    }
}
