using System;
using System.Linq;
using System.Numerics;
using System.Reflection;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Utility;
using Dalamud.Utility;

namespace YapYapDraw.Windows;

public sealed class HomeView
{
    private readonly Plugin _plugin;
    private static readonly string Version =
        Assembly.GetExecutingAssembly().GetName().Version?.ToString(3) ?? "1.0.0";

    private const string GithubUrl  = "https://github.com/ResFox/YapYapDraw";
    private const string DiscordUrl = "https://discord.gg/KZBxpkqbh4";
    private const string KofiUrl    = "https://ko-fi.com";

    public HomeView(Plugin plugin) => _plugin = plugin;

    public void Draw()
    {
        float w = ImGui.GetContentRegionAvail().X;

        ImGui.Dummy(new Vector2(0, 10f * ImGuiHelpers.GlobalScale));

        var logo = Assets.Logo;
        if (logo != null)
        {
            float aspect = logo.Height / (float)logo.Width;
            float lw = MathF.Min(168f * ImGuiHelpers.GlobalScale, w * 0.42f);
            float lh = lw * aspect;
            Center(lw, w);

            var p = ImGui.GetCursorScreenPos();
            var dl = ImGui.GetWindowDrawList();
            dl.AddRectFilled(p - new Vector2(6, 6), p + new Vector2(lw + 6, lh + 6),
                ImGui.ColorConvertFloat4ToU32(new Vector4(0.130f, 0.130f, 0.130f, 0.55f)), 14f);
            dl.AddRect(p - new Vector2(6, 6), p + new Vector2(lw + 6, lh + 6),
                ImGui.ColorConvertFloat4ToU32(Ui.Accent with { W = 0.35f }), 14f, ImDrawFlags.None, 1.5f);
            ImGui.Image(logo.Handle, new Vector2(lw, lh));
        }
        else ImGui.Dummy(new Vector2(0, 24f * ImGuiHelpers.GlobalScale));

        ImGui.Dummy(new Vector2(0, 6f * ImGuiHelpers.GlobalScale));
        CenterText("YapYap Draw", 1.9f, Ui.Blue, w);
        CenterText("Draw your mechanics on the arena floor.", 1f, Ui.Dimmed, w);

        ImGui.Dummy(new Vector2(0, 12f * ImGuiHelpers.GlobalScale));
        ImGui.Dummy(new Vector2(0, 4f * ImGuiHelpers.GlobalScale));
        AccentRule(w);
        ImGui.Dummy(new Vector2(0, 12f * ImGuiHelpers.GlobalScale));

        float btnW = 120f * ImGuiHelpers.GlobalScale;
        float spacing = ImGui.GetStyle().ItemSpacing.X;
        Center(btnW * 3 + spacing * 2, w);
        if (IconButton(FontAwesomeIcon.Code, "GitHub", btnW))  Util.OpenLink(GithubUrl);
        ImGui.SameLine();
        if (IconButton(FontAwesomeIcon.Comments, "Discord", btnW)) Util.OpenLink(DiscordUrl);
        ImGui.SameLine();
        if (IconButton(FontAwesomeIcon.Heart, "Ko-fi", btnW))   Util.OpenLink(KofiUrl);

        ImGui.Dummy(new Vector2(0, 16f * ImGuiHelpers.GlobalScale));

        DrawGuide(w);
    }

    private static bool IconButton(FontAwesomeIcon icon, string label, float width)
    {
        ImGui.PushFont(UiBuilder.IconFont);
        string ico = icon.ToIconString();
        float iw = ImGui.CalcTextSize(ico).X;
        ImGui.PopFont();
        float lw = ImGui.CalcTextSize(label).X;
        float pad = ImGui.GetStyle().FramePadding.X;

        var p = ImGui.GetCursorScreenPos();
        bool clicked = ImGui.Button($"##{label}", new Vector2(width, 0));
        float h = ImGui.GetItemRectSize().Y;

        float total = iw + 6f + lw;
        float sx = p.X + (width - total) * 0.5f;
        float ty = p.Y + (h - ImGui.GetTextLineHeight()) * 0.5f;
        var dl = ImGui.GetWindowDrawList();
        uint col = ImGui.ColorConvertFloat4ToU32(new Vector4(1f, 1f, 1f, 1f));
        ImGui.PushFont(UiBuilder.IconFont);
        dl.AddText(new Vector2(sx, ty), col, ico);
        ImGui.PopFont();
        dl.AddText(new Vector2(sx + iw + 6f, ty), col, label);
        return clicked;
    }

    private void DrawGuide(float w)
    {
        float cardW = MathF.Min(560f * ImGuiHelpers.GlobalScale, w);
        Center(cardW, w);
        ImGui.BeginGroup();

        if (GuideCard(cardW, FontAwesomeIcon.LayerGroup, "Modules",
                "Browse fights by category and toggle each mechanic on or off."))
            _plugin.ShowTab("modules");

        ImGui.Dummy(new Vector2(0, 6f * ImGuiHelpers.GlobalScale));

        if (GuideCard(cardW, FontAwesomeIcon.PaintBrush, "Quick Draws",
                "Make your own floor shapes from any cast, debuff, or marker."))
            _plugin.ShowTab("quickdraws");

        ImGui.Dummy(new Vector2(0, 6f * ImGuiHelpers.GlobalScale));

        if (GuideCard(cardW, FontAwesomeIcon.ListUl, "Fight Log",
                "Watch casts, statuses, markers, and tethers as they happen."))
            _plugin.ShowTab("log");

        ImGui.Dummy(new Vector2(0, 6f * ImGuiHelpers.GlobalScale));

        if (GuideCard(cardW, FontAwesomeIcon.BookOpen, "Library",
                "Everything this duty has shown — turn any line into a draw."))
            _plugin.ShowTab("library");

        ImGui.Dummy(new Vector2(0, 6f * ImGuiHelpers.GlobalScale));

        if (GuideCard(cardW, FontAwesomeIcon.Cog, "Settings",
                "Capture mode, debug overlay, and omen color saturation."))
            _plugin.OpenConfig();

        ImGui.EndGroup();
    }

    private static bool GuideCard(float width, FontAwesomeIcon icon, string title, string desc)
    {
        float h = ImGui.GetTextLineHeightWithSpacing() * 2.6f;
        var p0 = ImGui.GetCursorScreenPos();
        var p1 = new Vector2(p0.X + width, p0.Y + h);

        ImGui.InvisibleButton($"##card_{title}", new Vector2(width, h));
        bool hover = ImGui.IsItemHovered();
        bool clicked = ImGui.IsItemClicked();

        var dl = ImGui.GetWindowDrawList();
        var bg = hover ? new Vector4(0.175f, 0.175f, 0.175f, 1f) : new Vector4(0.130f, 0.130f, 0.130f, 1f);
        dl.AddRectFilled(p0, p1, ImGui.ColorConvertFloat4ToU32(bg), 8f);
        dl.AddRect(p0, p1, ImGui.ColorConvertFloat4ToU32(hover ? Ui.Accent with { W = 0.6f } : new Vector4(0.30f, 0.30f, 0.30f, 0.45f)),
            8f, ImDrawFlags.None, 1f);

        float pad = 14f * ImGuiHelpers.GlobalScale;
        ImGui.PushFont(UiBuilder.IconFont);
        string ico = icon.ToIconString();
        dl.AddText(new Vector2(p0.X + pad, p0.Y + (h - ImGui.GetTextLineHeight()) * 0.5f),
            ImGui.ColorConvertFloat4ToU32(Ui.Accent), ico);
        float icoW = ImGui.CalcTextSize(ico).X;
        ImGui.PopFont();

        float tx = p0.X + pad + icoW + 12f;
        dl.AddText(new Vector2(tx, p0.Y + 7f),
            ImGui.ColorConvertFloat4ToU32(new Vector4(1f, 1f, 1f, 1f)), title);
        dl.AddText(new Vector2(tx, p0.Y + 7f + ImGui.GetTextLineHeight() + 2f),
            ImGui.ColorConvertFloat4ToU32(Ui.Dimmed), desc);

        float chevW;
        ImGui.PushFont(UiBuilder.IconFont);
        string chev = FontAwesomeIcon.ChevronRight.ToIconString();
        chevW = ImGui.CalcTextSize(chev).X;
        dl.AddText(new Vector2(p1.X - pad - chevW, p0.Y + (h - ImGui.GetTextLineHeight()) * 0.5f),
            ImGui.ColorConvertFloat4ToU32(hover ? Ui.Accent : Ui.Dimmed), chev);
        ImGui.PopFont();

        return clicked;
    }

    private static void AccentRule(float avail)
    {
        ImGui.Dummy(new Vector2(0, 8f * ImGuiHelpers.GlobalScale));
        float lineW = MathF.Min(220f * ImGuiHelpers.GlobalScale, avail * 0.5f);
        Center(lineW, avail);
        var draw = ImGui.GetWindowDrawList();
        var p = ImGui.GetCursorScreenPos();
        float y = p.Y;
        float thick = 2f;
        uint cMid  = ImGui.ColorConvertFloat4ToU32(Ui.Blue);
        uint cEdge = ImGui.ColorConvertFloat4ToU32(Ui.Blue with { W = 0f });
        float half = lineW * 0.5f;
        draw.AddRectFilledMultiColor(new Vector2(p.X, y), new Vector2(p.X + half, y + thick),
            cEdge, cMid, cMid, cEdge);
        draw.AddRectFilledMultiColor(new Vector2(p.X + half, y), new Vector2(p.X + lineW, y + thick),
            cMid, cEdge, cEdge, cMid);
        ImGui.Dummy(new Vector2(lineW, thick));
    }

    private static void Center(float itemWidth, float avail)
    {
        float off = (avail - itemWidth) * 0.5f;
        if (off > 0) ImGui.SetCursorPosX(ImGui.GetCursorPosX() + off);
    }

    private static void CenterText(string text, float scale, Vector4 color, float avail)
    {
        ImGui.SetWindowFontScale(scale);
        float tw = ImGui.CalcTextSize(text).X;
        Center(tw, avail);
        ImGui.TextColored(color, text);
        ImGui.SetWindowFontScale(1f);
    }
}
