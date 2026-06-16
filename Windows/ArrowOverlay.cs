using System;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility;
using YapYapDraw.Engine.Helper;

namespace YapYapDraw.Windows;

public sealed class ArrowOverlay
{
    private const float Spread = 2.6179938f; // 150 degrees off the spine

    private readonly Plugin _plugin;

    public ArrowOverlay(Plugin plugin) => _plugin = plugin;

    public void Draw()
    {
        if (!_plugin.Configuration.QuickDrawsEnabled) return;

        var   dl    = ImGui.GetBackgroundDrawList();
        float scale = ImGuiHelpers.GlobalScale;

        foreach (var a in _plugin.Engine.ActiveArrows())
        {
            uint  col = ImGui.ColorConvertFloat4ToU32(a.Color);
            float th  = MathF.Max(1f, a.Thickness * scale);

            var fwd   = new Vector3(MathF.Sin(a.Angle),          0f, MathF.Cos(a.Angle));
            var left  = new Vector3(MathF.Sin(a.Angle + Spread), 0f, MathF.Cos(a.Angle + Spread));
            var right = new Vector3(MathF.Sin(a.Angle - Spread), 0f, MathF.Cos(a.Angle - Spread));
            var end   = a.Origin + fwd * a.Length;

            StrokeWorldLine(dl, a.Origin, end, th, col);

            if (a.Chevron)
            {
                int n = (int)(a.Length / a.Spacing);
                for (int i = 1; i <= n; i++)
                {
                    var c = a.Origin + fwd * (i * a.Spacing);
                    StrokeWorldLine(dl, c, c + left  * a.Spacing * 0.5f, th, col);
                    StrokeWorldLine(dl, c, c + right * a.Spacing * 0.5f, th, col);
                }
                if (PositionHelper.StableWorldToScreen(end, out var es))
                    dl.AddCircleFilled(es, MathF.Max(3f, th * 1.2f), col);
            }
            else
            {
                StrokeWorldLine(dl, end, end + left  * a.HeadSize, th, col);
                StrokeWorldLine(dl, end, end + right * a.HeadSize, th, col);
            }
        }
    }

    // Sample the segment per yalm so it tracks ground perspective, and break the
    // stroke wherever a point can't be projected (behind the camera).
    private static void StrokeWorldLine(ImDrawListPtr dl, Vector3 start, Vector3 end, float thickness, uint col)
    {
        float dist  = Vector3.Distance(start, end);
        int   steps = Math.Max(1, (int)MathF.Ceiling(dist));
        Vector2? prev = null;
        for (int i = 0; i <= steps; i++)
        {
            var w = Vector3.Lerp(start, end, (float)i / steps);
            if (PositionHelper.StableWorldToScreen(w, out var sp))
            {
                if (prev.HasValue) dl.AddLine(prev.Value, sp, col, thickness);
                prev = sp;
            }
            else
            {
                prev = null;
            }
        }
    }
}
