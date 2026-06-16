using System;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility;

namespace YapYapDraw.Windows;

public sealed class LabelOverlay
{
    private readonly Plugin _plugin;

    public LabelOverlay(Plugin plugin) => _plugin = plugin;

    private static readonly (float X, float Y)[] Dirs =
    {
        (-1f, 0f), (1f, 0f), (0f, -1f), (0f, 1f),
        (-0.7f, -0.7f), (0.7f, -0.7f), (-0.7f, 0.7f), (0.7f, 0.7f),
    };

    public void Draw()
    {
        if (!_plugin.Configuration.QuickDrawsEnabled) return;

        var font = _plugin.LabelFont;
        IDisposable? pushed = font is { Available: true } ? font.Push() : null;
        try
        {
            var   dl       = ImGui.GetBackgroundDrawList();
            var   f        = ImGui.GetFont();
            float native   = ImGui.GetFontSize();
            float baseSize = 24f * ImGuiHelpers.GlobalScale;
            uint  outline  = ImGui.ColorConvertFloat4ToU32(new Vector4(0f, 0f, 0f, 0.95f));

            _plugin.Engine.RefreshLabelScreens();

            foreach (var (screen, text, color, size) in _plugin.Engine.ActiveLabelScreens())
            {
                if (string.IsNullOrEmpty(text)) continue;

                float fsz = baseSize * MathF.Max(0.1f, size);
                float k   = fsz / native;
                var   ts  = ImGui.CalcTextSize(text) * k;
                var   pos = new Vector2(screen.X - ts.X * 0.5f, screen.Y - ts.Y * 0.5f);
                uint  col = ImGui.ColorConvertFloat4ToU32(color);
                float th  = MathF.Max(1.5f, fsz * 0.06f);

                foreach (var (dx, dy) in Dirs)
                    dl.AddText(f, fsz, pos + new Vector2(dx * th, dy * th), outline, text);
                dl.AddText(f, fsz, pos, col, text);
            }
        }
        finally
        {
            pushed?.Dispose();
        }
    }
}
