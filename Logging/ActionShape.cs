using System.Globalization;
using System.Text.RegularExpressions;
using Lumina.Text.ReadOnly;
using YapYapDraw.QuickDraws;
using LuminaAction = Lumina.Excel.Sheets.Action;

namespace YapYapDraw.Logging;

public static class ActionShape
{
    public readonly record struct Info(string Label, string Call);

    public readonly record struct Geom(QuickShape Shape, float Radius, float HalfWidth, float Length, int FanAngle);

    public static Geom? Resolve(uint actionId)
    {
        if (actionId == 0) return null;
        var row = Plugin.Actions.GetRowOrDefault(actionId);
        if (row == null) return null;

        float range = row.Value.EffectRange;
        float width = row.Value.XAxisModifier;
        return row.Value.CastType switch
        {
            2 => new Geom(QuickShape.Circle,    range, 0f, 0f, 0),
            3 => new Geom(QuickShape.Fan,       range, 0f, 0f, ConeAngle(row.Value)),
            4 => new Geom(QuickShape.Rectangle, 0f, System.MathF.Max(0.5f, width * 0.5f), range, 0),
            5 => new Geom(QuickShape.Donut,     range, 0f, 0f, 0),
            _ => null,
        };
    }

    public static Info? Describe(uint actionId)
    {
        var g = Resolve(actionId);
        if (g == null) return null;
        var v = g.Value;
        return v.Shape switch
        {
            QuickShape.Circle    => new Info($"● {Fmt(v.Radius)}", $".drawCircle({Fmt(v.Radius)})"),
            QuickShape.Fan       => new Info($"◔ {Fmt(v.Radius)} {v.FanAngle}°", $".drawCone({Fmt(v.Radius)}, {v.FanAngle})"),
            QuickShape.Rectangle => new Info($"▭ {Fmt(v.Length)}×{Fmt(v.HalfWidth * 2f)}", $".drawLine({Fmt(v.Length)}, {Fmt(v.HalfWidth)})"),
            QuickShape.Donut     => new Info($"◎ {Fmt(v.Radius)}", $".drawDonut(?, {Fmt(v.Radius)})"),
            _                    => null,
        };
    }

    private static int ConeAngle(LuminaAction row)
    {
        float angle = 90f;
        var omen = row.Omen.ValueNullable;
        if (omen != null)
        {
            var path = omen.Value.Path.ExtractText();
            var m = Regex.Match(path, @"fan(\d+)", RegexOptions.IgnoreCase);
            if (m.Success && float.TryParse(m.Groups[1].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var deg))
                angle = deg;
        }
        return (int)angle;
    }

    private static string Fmt(float v) =>
        v.ToString(v % 1f == 0f ? "0" : "0.##", CultureInfo.InvariantCulture);
}
