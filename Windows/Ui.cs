using System.Numerics;
using Dalamud.Bindings.ImGui;

namespace YapYapDraw.Windows;

public static class Ui
{
    public static readonly Vector4 Accent = new(0.843f, 0.247f, 0.290f, 1f);
    public static readonly Vector4 Blue   = Accent;
    public static readonly Vector4 Gold   = new(1.00f, 0.76f, 0.24f, 1f);
    public static readonly Vector4 Dimmed = new(0.58f, 0.56f, 0.55f, 1f);
    public static readonly Vector4 Green  = new(0.36f, 0.82f, 0.45f, 1f);
    public static readonly Vector4 Red    = new(0.96f, 0.42f, 0.42f, 1f);

    private const int ThemeColors = 29;
    private const int ThemeVars   = 9;

    public static void PushTheme()
    {
        ImGui.PushStyleVar(ImGuiStyleVar.WindowRounding,    9f);
        ImGui.PushStyleVar(ImGuiStyleVar.ChildRounding,     7f);
        ImGui.PushStyleVar(ImGuiStyleVar.FrameRounding,     5f);
        ImGui.PushStyleVar(ImGuiStyleVar.PopupRounding,     6f);
        ImGui.PushStyleVar(ImGuiStyleVar.GrabRounding,      4f);
        ImGui.PushStyleVar(ImGuiStyleVar.TabRounding,       6f);
        ImGui.PushStyleVar(ImGuiStyleVar.ScrollbarRounding, 8f);
        ImGui.PushStyleVar(ImGuiStyleVar.FramePadding,  new Vector2(8f, 5f));
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(12f, 10f));

        Col(ImGuiCol.Text,                new(0.960f, 0.960f, 0.960f, 1f));
        Col(ImGuiCol.TextDisabled,        new(0.550f, 0.550f, 0.550f, 1f));
        Col(ImGuiCol.WindowBg,            new(0.082f, 0.082f, 0.082f, 0.94f));
        Col(ImGuiCol.ChildBg,             new(0.120f, 0.120f, 0.120f, 0.45f));
        Col(ImGuiCol.PopupBg,             new(0.100f, 0.100f, 0.100f, 0.96f));
        Col(ImGuiCol.Border,              new(0.843f, 0.247f, 0.290f, 0.22f));
        Col(ImGuiCol.FrameBg,             new(0.160f, 0.160f, 0.160f, 1f));
        Col(ImGuiCol.FrameBgHovered,      new(0.220f, 0.205f, 0.207f, 1f));
        Col(ImGuiCol.FrameBgActive,       new(0.280f, 0.255f, 0.258f, 1f));
        Col(ImGuiCol.TitleBg,             new(0.100f, 0.100f, 0.100f, 1f));
        Col(ImGuiCol.TitleBgActive,       new(0.220f, 0.080f, 0.100f, 1f));
        Col(ImGuiCol.TitleBgCollapsed,    new(0.100f, 0.100f, 0.100f, 0.75f));
        Col(ImGuiCol.Button,              new(0.200f, 0.200f, 0.200f, 1f));
        Col(ImGuiCol.ButtonHovered,       new(0.550f, 0.180f, 0.210f, 1f));
        Col(ImGuiCol.ButtonActive,        new(0.843f, 0.247f, 0.290f, 1f));
        Col(ImGuiCol.Header,              new(0.200f, 0.180f, 0.183f, 1f));
        Col(ImGuiCol.HeaderHovered,       new(0.500f, 0.170f, 0.200f, 1f));
        Col(ImGuiCol.HeaderActive,        new(0.780f, 0.235f, 0.275f, 1f));
        Col(ImGuiCol.CheckMark,           new(0.950f, 0.350f, 0.380f, 1f));
        Col(ImGuiCol.SliderGrab,          new(0.700f, 0.220f, 0.255f, 1f));
        Col(ImGuiCol.SliderGrabActive,    new(0.920f, 0.300f, 0.340f, 1f));
        Col(ImGuiCol.Separator,           new(0.240f, 0.240f, 0.240f, 1f));
        Col(ImGuiCol.SeparatorHovered,    new(0.843f, 0.247f, 0.290f, 0.70f));
        Col(ImGuiCol.Tab,                 new(0.130f, 0.130f, 0.130f, 1f));
        Col(ImGuiCol.TabHovered,          new(0.550f, 0.180f, 0.210f, 1f));
        Col(ImGuiCol.TabActive,           new(0.320f, 0.110f, 0.130f, 1f));
        Col(ImGuiCol.ScrollbarBg,         new(0.080f, 0.080f, 0.080f, 0.60f));
        Col(ImGuiCol.ScrollbarGrab,       new(0.240f, 0.240f, 0.240f, 1f));
        Col(ImGuiCol.ScrollbarGrabHovered,new(0.550f, 0.180f, 0.210f, 1f));
    }

    public static void PopTheme()
    {
        ImGui.PopStyleColor(ThemeColors);
        ImGui.PopStyleVar(ThemeVars);
    }

    private static void Col(ImGuiCol idx, Vector4 c) => ImGui.PushStyleColor(idx, c);

    public static void NavBar(Plugin plugin, string current)
    {
        if (current != "log")
        { if (ImGui.Button("Log")) plugin.ToggleLog(); ImGui.SameLine(); }
        if (current != "modules")
        { if (ImGui.Button("Modules")) plugin.ShowTab("modules"); ImGui.SameLine(); }
        if (ImGui.Button("Settings")) plugin.OpenConfig();
        ImGui.Separator();
    }

    public static bool ToggleSwitch(string id, ref bool value)
    {
        var draw = ImGui.GetWindowDrawList();
        float frame = ImGui.GetFrameHeight();
        float h = frame * 0.82f;
        float w = h * 1.8f;
        float r = h * 0.5f;

        var origin = ImGui.GetCursorScreenPos();
        ImGui.InvisibleButton(id, new Vector2(w, frame));

        bool changed = false;
        if (ImGui.IsItemClicked()) { value = !value; changed = true; }
        bool hovered = ImGui.IsItemHovered();

        float yOff = (frame - h) * 0.5f;
        var p0 = new Vector2(origin.X, origin.Y + yOff);
        var p1 = new Vector2(origin.X + w, origin.Y + yOff + h);

        var on  = Accent with { W = hovered ? 1f : 0.92f };
        var off = new Vector4(0.32f, 0.32f, 0.32f, hovered ? 1f : 0.9f);
        draw.AddRectFilled(p0, p1, ImGui.ColorConvertFloat4ToU32(value ? on : off), r);

        float cx = value ? p1.X - r : p0.X + r;
        draw.AddCircleFilled(new Vector2(cx, p0.Y + r), r - 2f,
            ImGui.ColorConvertFloat4ToU32(new Vector4(1f, 1f, 1f, 1f)), 24);

        return changed;
    }
}
