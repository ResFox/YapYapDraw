using System;
using Dalamud.Plugin.Services;
using YapYapDraw.Engine.Interop;
using Lumina.Excel.Sheets;
using YapYapDraw.Engine.Enum;
using Action = Lumina.Excel.Sheets.Action;

namespace YapYapDraw.Engine.Util;

public static class ShapeUtil
{
    public static Shape GetShape(byte actionType)
    {
        return actionType switch
        {
            2 => Shape.Circle,
            3 => Shape.Cone,
            4 => Shape.Rectangle,
            5 => Shape.Circle,
            7 => Shape.Circle,
            8 => Shape.RectToTarget,
            10 => Shape.Donut,
            11 => Shape.Cross,
            12 => Shape.Rectangle,
            13 => Shape.Cone,
            14 => Shape.Triangle,
            _ => Shape.None,
        };
    }

    public static string GetGameTriangleOmen(int Degree)
    {
        return Degree switch
        {
            30 => "x6d3_b2_triangle30_p1",
            60 => "x6d3_b2_triangle60_p1",
            90 => "x6d3_b2_triangle90_p1",
            _ => string.Empty,
        };
    }

    public static string GetGameFanOmen(int Degree)
    {
        return Degree switch
        {
            15 => "gl_fan015_0x",
            20 => "gl_fan020_0f",
            30 => "gl_fan030_1bf",
            40 => "gl_fan045_1bf",
            45 => "gl_fan045_1bf",
            60 => "gl_fan060_1bf",
            90 => "gl_fan090_1bf",
            120 => "gl_fan120_1bf",
            130 => "gl_fan130_0x",
            135 => "gl_fan135_c0g",
            150 => "gl_fan150_1bf",
            180 => "gl_fan180_1bf",
            210 => "gl_fan210_1bf",
            240 => "x6d3_b1_fan240_p1",
            270 => "gl_fan270_0100af",
            _ => "customFan",
        };
    }

    public static int DetermineConeAngle(Action data)
    {
        Omen omen = data.Omen.Value;
        if (omen.RowId == 0)
        {
            Svc.Log.Warning($"No omen data for {data.RowId} '{data.Name}'...");
            return 90;
        }

        string path = omen.Path.ToString();
        int idx = path.IndexOf("fan", StringComparison.Ordinal);
        if (idx < 0 || idx + 6 > path.Length)
        {
            Svc.Log.Warning($"Can't determine angle from omen ({path}/{omen.PathAlly}) for {data.RowId} '{data.Name}'...");
            return 90;
        }

        if (!int.TryParse(path.AsSpan(idx + 3, 3), out int angle))
        {
            Svc.Log.Warning($"Can't determine angle from omen ({path}/{omen.PathAlly}) for {data.RowId} '{data.Name}'...");
            return 90;
        }

        Plugin.DebugLog($"{data.Name}({data.RowId}) Omen:({path}/{omen.PathAlly}) Degrees:{angle}° {angle.Degrees()}");
        return angle;
    }

    public static float RadiansToDegrees(this float radians)
    {
        return 360f - ((float)(180.0 / Math.PI * radians) + 180f);
    }

    public static float IntToFloatAngle(this ushort rot)
    {
        return rot / 65535f * (MathF.PI * 2f) - MathF.PI;
    }

    public static int DetermineTriangleAngle(Action data)
    {
        Omen omen = data.Omen.Value;
        if (omen.RowId == 0)
        {
            Svc.Log.Warning($"No omen data for {data.RowId} '{data.Name}'...");
            return 90;
        }

        string path = omen.Path.ToString();
        int idx = path.IndexOf("triangle", StringComparison.Ordinal);
        if (idx < 0 || idx + 9 > path.Length)
        {
            Svc.Log.Warning($"Can't determine angle from omen ({path}/{omen.PathAlly}) for {data.RowId} '{data.Name}'...");
            return 90;
        }

        if (!int.TryParse(path.AsSpan(idx + 8, 2), out int angle) && !int.TryParse(path.AsSpan(idx + 8, 3), out angle))
        {
            Svc.Log.Warning($"Can't determine angle from omen ({path}/{omen.PathAlly}) for {data.RowId} '{data.Name}'...");
            return 90;
        }

        Plugin.DebugLog($"{data.Name}({data.RowId}) Omen:({path}/{omen.PathAlly}) Degrees:{angle}° {angle.Degrees()}");
        return angle;
    }
}
