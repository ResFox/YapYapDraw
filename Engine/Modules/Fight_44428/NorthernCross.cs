using System.Collections.Generic;
using System.Numerics;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Util;

namespace YapYapDraw.Modules.Fight_44428;

public class NorthernCross : ISpecialAction
{
    public override string Name => "Northern Cross";

    public override HashSet<uint> ActionID => new HashSet<uint>();

    public override void OnEnvControl(byte index, uint state)
    {
        if (index == 2)
        {
            Angle angle = state switch
            {
                2097168u => -90.Degrees(), 
                131073u => 90.Degrees(), 
                _ => default, 
            };
            if (angle != default)
            {
                DrawManager.Draw(new DrawElement
                {
                    drawAvfx = "general02f",
                    Position = new Vector3(100f, 0f, 100f),
                    drawOnObject = false,
                    radiusX = 30f,
                    radiusZ = 25f,
                    refRotation = -126.875f.Degrees() + angle,
                    fixRotation = true,
                    hitCounter = new HitCounter
                    {
                        ActionID = new HashSet<uint> { 36168u, 36169u }
                    }
                });
            }
        }
    }
}
