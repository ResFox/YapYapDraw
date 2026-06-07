using System;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Windowing;

namespace YapYapDraw.Windows;

public sealed class ConfigWindow : Window, IDisposable
{
    private readonly Plugin _plugin;
    private static readonly string[] CaptureNames = { "Always", "In combat", "In a duty" };

    public ConfigWindow(Plugin plugin)
        : base("YapYap Draw Settings###YapYapDrawConfig",
               ImGuiWindowFlags.AlwaysAutoResize)
    {
        _plugin = plugin;
    }

    public void Dispose() { }

    private static void Sep(string label)
    {
        ImGui.Spacing();
        ImGui.Separator();
        ImGui.TextDisabled(label);
    }

    public override void Draw() => DrawContent();

    private void DrawHealth()
    {
        var cap = _plugin.Capture;
        bool core    = cap.ActionEffectInstalled;
        bool markers = cap.ActorControlInstalled;
        bool mapfx   = cap.MapEffectInstalled;

        if (core && markers && mapfx)
        {
            ImGui.TextColored(Ui.Green, "● Connected");
            return;
        }

        HookRow("Casts & statuses", core);
        HookRow("Headmarkers & tethers", markers);
        HookRow("Arena map effects", mapfx);

        if (!core)
            ImGui.TextColored(Ui.Red,
                "Core detection is down — likely a game patch. Tell the dev which lines are red.");
        else if (!markers || !mapfx)
            ImGui.TextColored(Ui.Dimmed,
                "Some optional feeds are down; casts & statuses still work.");
    }

    private static void HookRow(string label, bool ok)
    {
        ImGui.TextColored(ok ? Ui.Green : Ui.Red, ok ? "  ● " : "  ✕ ");
        ImGui.SameLine(0, 0);
        ImGui.TextColored(ok ? Ui.Dimmed : Ui.Red, $"{label}: {(ok ? "OK" : "NOT WORKING")}");
    }

    private static void ToggleRow(string label, string desc, ref bool value, out bool changed)
    {
        changed = Ui.ToggleSwitch("##t_" + label, ref value);
        ImGui.SameLine(0, 10f);
        ImGui.AlignTextToFramePadding();
        ImGui.TextColored(value ? new System.Numerics.Vector4(1f, 1f, 1f, 1f) : Ui.Dimmed, label);
        if (!string.IsNullOrEmpty(desc))
        {
            float indent = 44f * ImGuiHelpers.GlobalScale;
            ImGui.Indent(indent);
            ImGui.TextDisabled(desc);
            ImGui.Unindent(indent);
        }
    }

    public void DrawContent()
    {
        var cfg = _plugin.Configuration;
        var host = _plugin.Host;

        Sep("Drawing");
        bool modulesOn = cfg.ModulesEnabled;
        ToggleRow("Enable mechanic drawing", "Master switch for all fight telegraphs.",
            ref modulesOn, out bool mChanged);
        if (mChanged) { cfg.ModulesEnabled = modulesOn; cfg.Save(); }

        ImGui.Spacing();
        float sat = cfg.CustomAlpha;
        ImGui.SetNextItemWidth(220f * ImGuiHelpers.GlobalScale);
        if (ImGui.SliderFloat("Omen opacity", ref sat, 1f, 2f))
        { cfg.CustomAlpha = sat; cfg.Save(); }
        ImGui.TextDisabled("Default 1.5. Higher is brighter, lower is more transparent.");

        Sep("Window");
        bool openLogin = cfg.OpenOnLogin;
        ToggleRow("Open on login", "Pop the main window automatically when you log in.",
            ref openLogin, out bool olChanged);
        if (olChanged) { cfg.OpenOnLogin = openLogin; cfg.Save(); }

        Sep("Capture");
        int idx = (int)cfg.CaptureWhen;
        ImGui.SetNextItemWidth(160f * ImGuiHelpers.GlobalScale);
        if (ImGui.Combo("When to read combat", ref idx, CaptureNames, CaptureNames.Length))
        { cfg.CaptureWhen = (CaptureMode)idx; cfg.Save(); }
        ImGui.TextDisabled("Limits when the plugin processes casts and effects.");

        ImGui.Spacing();
        bool debugHud = cfg.DebugHud;
        ToggleRow("Show debug overlay", "On-screen counters for active fight and captured events.",
            ref debugHud, out bool dChanged);
        if (dChanged) { cfg.DebugHud = debugHud; cfg.Save(); }

        Sep("Toggled off");
        int offFights = cfg.DisabledFights.Count;
        int offMechs  = cfg.DisabledMechanics.Count;
        if (offFights == 0 && offMechs == 0)
            ImGui.TextDisabled("Everything is enabled.");
        else
        {
            ImGui.TextDisabled($"{offFights} fight(s) and {offMechs} mechanic(s) turned off.");
            ImGui.Spacing();
            if (ImGui.Button("Re-enable everything"))
            {
                cfg.DisabledFights.Clear();
                cfg.DisabledMechanics.Clear();
                cfg.Save();
            }
        }

        Sep("Game data");
        DrawHealth();

        Sep("Engine");
        ImGui.TextColored(host.HooksInstalled ? Ui.Green : Ui.Red,
            host.HooksInstalled ? "● Omen engine ready" : "✕ Omen engine failed to init");
        ImGui.TextDisabled($"Active fight: {host.FightName}  ·  Territory {host.TerritoryId}");
    }
}
