using System;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Windowing;

namespace YapYapDraw.Windows;

public sealed class ChangelogWindow : Window, IDisposable
{
    private readonly Plugin _plugin;

    public ChangelogWindow(Plugin plugin)
        : base("YapYap Draw - What's New###YapYapDrawChangelog",
               ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoResize)
    {
        _plugin = plugin;
        Size = new Vector2(560, 380);
        SizeCondition = ImGuiCond.Appearing;
    }

    public void Dispose() { }

    public override void PreDraw()  => Ui.PushTheme();
    public override void PostDraw() => Ui.PopTheme();

    public override void Draw()
    {
        float scale = ImGuiHelpers.GlobalScale;
        float w     = ImGui.GetContentRegionAvail().X;
        var   dl    = ImGui.GetWindowDrawList();

        ImGui.Dummy(new Vector2(0, 6f * scale));

        ImGui.SetWindowFontScale(1.7f);
        float tw = ImGui.CalcTextSize(Changelog.Title).X;
        ImGui.SetCursorPosX(ImGui.GetCursorPosX() + (w - tw) * 0.5f);
        ImGui.TextColored(Ui.Blue, Changelog.Title);
        ImGui.SetWindowFontScale(1f);

        string ver = $"v{Changelog.Version}";
        float vw = ImGui.CalcTextSize(ver).X;
        ImGui.SetCursorPosX(ImGui.GetCursorPosX() + (w - vw) * 0.5f);
        ImGui.TextColored(Ui.Dimmed, ver);

        ImGui.Dummy(new Vector2(0, 10f * scale));
        AccentRule(w, scale);
        ImGui.Dummy(new Vector2(0, 12f * scale));

        float pad = 6f * scale;
        foreach (var note in Changelog.Notes)
        {
            var p0 = ImGui.GetCursorScreenPos();
            ImGui.Indent(10f * scale);

            float dotR = 3.5f * scale;
            var   dotP = new Vector2(p0.X + 14f * scale, ImGui.GetCursorScreenPos().Y + ImGui.GetTextLineHeight() * 0.5f);
            dl.AddCircleFilled(dotP, dotR, ImGui.ColorConvertFloat4ToU32(Ui.Gold));

            ImGui.Indent(20f * scale);
            ImGui.PushTextWrapPos(ImGui.GetCursorPosX() + (w - 40f * scale));
            ImGui.TextWrapped(note);
            ImGui.PopTextWrapPos();
            ImGui.Unindent(30f * scale);

            ImGui.Dummy(new Vector2(0, pad));
        }

        float btnW = 160f * scale;
        float btnH = 32f * scale;
        ImGui.SetCursorPosY(ImGui.GetWindowHeight() - btnH - 16f * scale);
        ImGui.SetCursorPosX((ImGui.GetWindowWidth() - btnW) * 0.5f);
        if (ImGui.Button("Got it", new Vector2(btnW, btnH)))
            IsOpen = false;
    }

    private static void AccentRule(float avail, float scale)
    {
        float lineW = MathF.Min(280f * scale, avail * 0.7f);
        ImGui.SetCursorPosX(ImGui.GetCursorPosX() + (avail - lineW) * 0.5f);
        var draw = ImGui.GetWindowDrawList();
        var p = ImGui.GetCursorScreenPos();
        float thick = 2f;
        uint cMid  = ImGui.ColorConvertFloat4ToU32(Ui.Blue);
        uint cEdge = ImGui.ColorConvertFloat4ToU32(Ui.Blue with { W = 0f });
        float half = lineW * 0.5f;
        draw.AddRectFilledMultiColor(new Vector2(p.X, p.Y), new Vector2(p.X + half, p.Y + thick),
            cEdge, cMid, cMid, cEdge);
        draw.AddRectFilledMultiColor(new Vector2(p.X + half, p.Y), new Vector2(p.X + lineW, p.Y + thick),
            cMid, cEdge, cEdge, cMid);
        ImGui.Dummy(new Vector2(lineW, thick));
    }

    public override void OnClose()
    {
        if (_plugin.Configuration.LastSeenVersion == Changelog.Version) return;
        _plugin.Configuration.LastSeenVersion = Changelog.Version;
        _plugin.Configuration.Save();
    }
}
