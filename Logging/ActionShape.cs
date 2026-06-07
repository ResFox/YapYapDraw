using System.Globalization;
using System.Text.RegularExpressions;
using Lumina.Text.ReadOnly;
using LuminaAction = Lumina.Excel.Sheets.Action;

namespace YapYapDraw.Logging;

public static class ActionShape
{
    public readonly record struct Info(string Label, string Call);

    public static Info? Describe(uint actionId)
    {
        if (actionId == 0) return null;
        var row = Plugin.DataManager.GetExcelSheet<LuminaAction>().GetRowOrDefault(actionId);
        if (row == null) return null;

        float range = row.Value.EffectRange;
        float width = row.Value.XAxisModifier;
        byte castType = row.Value.CastType;

        return castType switch
        {
            2 => new Info($"● {Fmt(range)}", $".drawCircle({Fmt(range)})"),
            3 => Cone(row.Value, range),
            4 => new Info($"▭ {Fmt(range)}×{Fmt(width)}", $".drawLine({Fmt(range)}, {Fmt(width * 0.5f)})"),
            5 => new Info($"◎ {Fmt(range)}", $".drawDonut(?, {Fmt(range)})"),
            _ => null,
        };
    }

    private static Info? Cone(LuminaAction row, float range)
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
        return new Info($"◔ {Fmt(range)} {angle:0}°", $".drawCone({Fmt(range)}, {Fmt(angle)})");
    }

    private static string Fmt(float v) =>
        v.ToString(v % 1f == 0f ? "0" : "0.##", CultureInfo.InvariantCulture);
}
