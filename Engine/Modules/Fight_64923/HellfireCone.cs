using System;
using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Util;

namespace YapYapDraw.Modules.Fight_64923;

public class HellfireCone : ISpecialAction
{
    private readonly byte[] indexs = new byte[9] { 5, 6, 7, 8, 9, 10, 11, 12, 13 };

    private byte lineIndex;

    public override string Name => "Hellfire (cone)";

    public override HashSet<uint> ActionID => new HashSet<uint>();

    public override uint Phase => 1u;

    public override void OnEnvControl(byte index, uint state)
    {
        if (index == 2)
        {
            if (base.NumCasts > 2 && (state == 131073 || state == 2097168))
            {
                int currentIndex = indexs.IndexOf(lineIndex);
                int offset = ((state != 131073) ? 1 : (-1));
                AddAoe((currentIndex + offset + 8) % 8);
            }
        }
        else if (indexs.Contains(index) && (state == 65540 || state == 131076 || state == 524292))
        {
            base.NumCasts++;
            if (base.NumCasts == 1)
            {
                AddAoe(indexs.IndexOf(index));
            }
            else if (base.NumCasts == 2)
            {
                int currentIndex = indexs.IndexOf(index);
                int offset = ((state == 131076) ? 1 : (-1));
                AddAoe((currentIndex + offset + 8) % 8);
            }
            else if (base.NumCasts > 2)
            {
                lineIndex = index;
            }
        }
    }

    private static void AddAoe(int index)
    {
        DrawElement drawElement = new DrawElement
        {
            drawAvfx = "m0611_fan_60x",
            radiusX = 40f,
            radiusZ = 40f,
            drawOnObject = false,
            hitCounter = new HitCounter
            {
                ActionID = new HashSet<uint> { 31941u }
            }
        };
        Angle angle = index switch
        {
            0 => 180.Degrees(), 
            1 => 135.Degrees(), 
            2 => 90.Degrees(), 
            3 => 45.Degrees(), 
            4 => 0.Degrees(), 
            5 => -45.Degrees(), 
            6 => -90.Degrees(), 
            7 => -135.Degrees(), 
            _ => default, 
        };
        WPos wPos = new WPos(100f, 100f) + 20f * angle.ToDirection();
        WDir wDir = new WPos(100f, 100f) - wPos;
        drawElement.Position = wPos.ToVec3();
        drawElement.refRotation = MathF.Atan2(wDir.X, wDir.Z).Radians();
        drawElement.fixRotation = true;
        DrawManager.Draw(drawElement);
    }
}
