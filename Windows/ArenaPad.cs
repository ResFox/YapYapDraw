using System;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Game.ClientState.Objects.Types;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using YapYapDraw.QuickDraws;

namespace YapYapDraw.Windows;

internal static class ArenaPad
{
    public const float CenterX = 100f;
    public const float CenterZ = 100f;
    public const float HalfExtent = 30f;
    public const float RaidRadius = 20f;

    public enum Shape : byte { Circle, Square }

    public static Shape DetectShape(uint territoryId)
    {
        if (territoryId == 0) return Shape.Square;

        var cat = ZoneLibrary.CategoryOf(territoryId);
        if (cat.Contains("Raid", StringComparison.OrdinalIgnoreCase) ||
            cat.Contains("Trial", StringComparison.OrdinalIgnoreCase) ||
            cat.Contains("Ultimate", StringComparison.OrdinalIgnoreCase) ||
            cat.Contains("Chaotic", StringComparison.OrdinalIgnoreCase))
            return Shape.Circle;

        return Shape.Square;
    }

    public static void Draw(
        string id,
        Plugin plugin,
        Func<Vector3> get,
        Action<Vector3> set,
        float scale,
        bool snapGrid,
        Action<bool> setSnapGrid,
        Action onDirty)
    {
        ImGui.PushID(id);
        var p = get();
        var xz = new Vector2(p.X, p.Z);
        ImGui.SetNextItemWidth(200f * scale);
        if (ImGui.InputFloat2("X / Z", ref xz))
        {
            set(new Vector3(xz.X, 0f, xz.Y));
            onDirty();
        }

        ImGui.SameLine();
        if (ImGui.SmallButton("Use my spot"))
        {
            var me = Plugin.ObjectTable.SearchById(Plugin.PlayerState.EntityId);
            if (me != null) { set(me.Position); onDirty(); }
        }

        bool snap = snapGrid;
        if (ImGui.Checkbox("Snap 1y", ref snap)) setSnapGrid(snap);

        ImGui.SameLine();
        if (ImGui.SmallButton("Pop out"))
            ImGui.OpenPopup($"##arena_pop_{id}");

        uint territory = Plugin.ClientState.TerritoryType;
        var shape = DetectShape(territory);
        ImGui.TextColored(Ui.Dimmed,
            shape == Shape.Circle
                ? $"circle arena  centre {CenterX:0},{CenterZ:0}  r {RaidRadius:0}y"
                : $"square pad  centre {CenterX:0},{CenterZ:0}  ±{HalfExtent:0}y");

        float padSize = 150f * scale;
        DrawInteractivePad(get, set, padSize, shape, plugin, snapGrid, onDirty);

        var popSize = 420f * scale;
        ImGui.SetNextWindowSize(new Vector2(popSize + 24f, popSize + 110f), ImGuiCond.FirstUseEver);
        if (ImGui.BeginPopupModal($"##arena_pop_{id}", ImGuiWindowFlags.NoScrollbar))
        {
            ImGui.TextColored(Ui.Dimmed, "Click to place — Esc or Close below");
            DrawInteractivePad(get, set, popSize, shape, plugin, snapGrid, onDirty);
            ImGui.Spacing();
            ImGui.Separator();
            ImGui.Spacing();
            if (ImGui.Button("Close", new Vector2(-1f, 36f * scale)))
                ImGui.CloseCurrentPopup();
            ImGui.EndPopup();
        }

        ImGui.PopID();
    }

    private static void DrawInteractivePad(
        Func<Vector3> get,
        Action<Vector3> set,
        float size,
        Shape shape,
        Plugin plugin,
        bool snapGrid,
        Action onDirty)
    {
        var origin = ImGui.GetCursorScreenPos();
        ImGui.InvisibleButton("##pad", new Vector2(size, size));
        var dl = ImGui.GetWindowDrawList();
        var a = origin;
        var b = new Vector2(origin.X + size, origin.Y + size);
        var mid = new Vector2(origin.X + size * 0.5f, origin.Y + size * 0.5f);

        dl.AddRectFilled(a, b, ImGui.ColorConvertFloat4ToU32(new Vector4(0.10f, 0.10f, 0.11f, 1f)), 4f);
        DrawGrid(dl, a, b, size);
        DrawArenaOutline(dl, a, b, mid, size, shape);

        Vector2 ToScreen(float wx, float wz) => WorldToScreen(wx, wz, origin, size);
        DrawOverlays(ToScreen, plugin);

        var dot = ToScreen(get().X, get().Z);
        dl.AddCircleFilled(dot, 5f, ImGui.ColorConvertFloat4ToU32(Ui.Accent));

        if (ImGui.IsItemActive() && ImGui.IsMouseDown(ImGuiMouseButton.Left))
        {
            var m = ImGui.GetMousePos();
            float wx = (CenterX - HalfExtent) + (m.X - origin.X) / size * (HalfExtent * 2f);
            float wz = (CenterZ - HalfExtent) + (m.Y - origin.Y) / size * (HalfExtent * 2f);
            if (snapGrid)
            {
                wx = MathF.Round(wx);
                wz = MathF.Round(wz);
            }
            else
            {
                wx = MathF.Round(wx, 1);
                wz = MathF.Round(wz, 1);
            }
            set(new Vector3(wx, 0f, wz));
            onDirty();
        }
    }

    private static void DrawGrid(ImDrawListPtr dl, Vector2 a, Vector2 b, float size)
    {
        uint minor = ImGui.ColorConvertFloat4ToU32(new Vector4(0.22f, 0.22f, 0.24f, 0.55f));
        uint major = ImGui.ColorConvertFloat4ToU32(new Vector4(0.30f, 0.30f, 0.33f, 0.85f));

        float minW = CenterX - HalfExtent;
        float maxW = CenterX + HalfExtent;
        for (float w = minW; w <= maxW + 0.01f; w += 1f)
        {
            float t = (w - minW) / (HalfExtent * 2f);
            float x = a.X + t * size;
            bool majorLine = MathF.Abs(w - CenterX) < 0.01f || MathF.Abs(MathF.Round(w) % 5) < 0.01f;
            dl.AddLine(new Vector2(x, a.Y), new Vector2(x, b.Y), majorLine ? major : minor);
        }
        for (float z = minW; z <= maxW + 0.01f; z += 1f)
        {
            float t = (z - minW) / (HalfExtent * 2f);
            float y = a.Y + t * size;
            bool majorLine = MathF.Abs(z - CenterZ) < 0.01f || MathF.Abs(MathF.Round(z) % 5) < 0.01f;
            dl.AddLine(new Vector2(a.X, y), new Vector2(b.X, y), majorLine ? major : minor);
        }
    }

    private static void DrawArenaOutline(ImDrawListPtr dl, Vector2 a, Vector2 b, Vector2 mid, float size, Shape shape)
    {
        uint outline = ImGui.ColorConvertFloat4ToU32(new Vector4(0.35f, 0.35f, 0.38f, 1f));
        if (shape == Shape.Circle)
        {
            float r = RaidRadius / (HalfExtent * 2f) * size;
            dl.AddCircle(mid, r, outline, 48, 1.5f);
        }
        else
            dl.AddRect(a, b, outline, 4f);
    }

    private static void DrawOverlays(Func<float, float, Vector2> toScreen, Plugin plugin)
    {
        var dl = ImGui.GetWindowDrawList();
        var me = Plugin.ObjectTable.LocalPlayer;
        if (me != null)
        {
            var sp = toScreen(me.Position.X, me.Position.Z);
            dl.AddCircleFilled(sp, 4f, ImGui.ColorConvertFloat4ToU32(new Vector4(0.3f, 0.85f, 1f, 0.95f)));
        }

        foreach (var hm in plugin.Capture.ActiveHeadmarkers)
        {
            var obj = Plugin.ObjectTable.SearchById(hm.ActorId);
            if (obj == null) continue;
            var sp = toScreen(obj.Position.X, obj.Position.Z);
            dl.AddCircle(sp, 6f, ImGui.ColorConvertFloat4ToU32(new Vector4(1f, 0.85f, 0.2f, 0.9f)), 10, 1.5f);
            dl.AddText(new Vector2(sp.X + 7f, sp.Y - 6f),
                ImGui.ColorConvertFloat4ToU32(new Vector4(1f, 0.9f, 0.5f, 0.9f)),
                $"{hm.IconId:X}");
        }

        foreach (var t in plugin.Capture.ActiveTethers)
            DrawTetherLine(dl, toScreen, t.From, t.To);

        DrawLiveTethers(dl, toScreen);
        DrawFieldMarkers(dl, toScreen);
    }

    private static readonly string[] WaymarkLabels =
        { "A", "B", "C", "D", "1", "2", "3", "4" };

    private static readonly Vector4[] WaymarkColors =
    {
        new(1f, 0.25f, 0.25f, 0.95f),
        new(0.25f, 0.85f, 0.35f, 0.95f),
        new(0.30f, 0.55f, 1f, 0.95f),
        new(1f, 0.85f, 0.20f, 0.95f),
        new(0.95f, 0.45f, 1f, 0.95f),
        new(0.35f, 0.95f, 0.95f, 0.95f),
        new(1f, 0.55f, 0.20f, 0.95f),
        new(0.75f, 0.75f, 0.80f, 0.95f),
    };

    private static unsafe void DrawFieldMarkers(ImDrawListPtr dl, Func<float, float, Vector2> toScreen)
    {
        var mc = MarkingController.Instance();
        if (mc == null) return;

        int i = 0;
        foreach (ref var marker in mc->FieldMarkers)
        {
            if (i >= WaymarkLabels.Length) break;
            if (marker.Active)
            {
                var sp = toScreen(marker.X / 1000f, marker.Z / 1000f);
                var col = ImGui.ColorConvertFloat4ToU32(WaymarkColors[i]);
                float half = 5f;
                dl.AddRectFilled(
                    new Vector2(sp.X - half, sp.Y - half),
                    new Vector2(sp.X + half, sp.Y + half), col, 1f);
                dl.AddRect(
                    new Vector2(sp.X - half, sp.Y - half),
                    new Vector2(sp.X + half, sp.Y + half),
                    ImGui.ColorConvertFloat4ToU32(new Vector4(1f, 1f, 1f, 0.85f)), 1f, ImDrawFlags.None, 1f);
                var label = WaymarkLabels[i];
                var ts = ImGui.CalcTextSize(label);
                dl.AddText(new Vector2(sp.X - ts.X * 0.5f, sp.Y - ts.Y * 0.5f),
                    ImGui.ColorConvertFloat4ToU32(new Vector4(0.05f, 0.05f, 0.05f, 1f)), label);
            }
            i++;
        }
    }

    private static void DrawTetherLine(ImDrawListPtr dl, Func<float, float, Vector2> toScreen, uint fromId, uint toId)
    {
        var from = Plugin.ObjectTable.SearchById(fromId);
        var to   = Plugin.ObjectTable.SearchById(toId);
        if (from == null || to == null) return;
        var a = toScreen(from.Position.X, from.Position.Z);
        var b = toScreen(to.Position.X, to.Position.Z);
        dl.AddLine(a, b, ImGui.ColorConvertFloat4ToU32(new Vector4(0.9f, 0.45f, 1f, 0.75f)), 1.5f);
    }

    private static unsafe void DrawLiveTethers(ImDrawListPtr dl, Func<float, float, Vector2> toScreen)
    {
        var seen = new System.Collections.Generic.HashSet<(uint, uint)>();
        foreach (var obj in Plugin.ObjectTable)
        {
            if (obj is not IBattleChara bc) continue;
            var chr = (Character*)bc.Address;
            if (chr == null) continue;
            var tether = chr->Vfx.Tethers[0];
            if (tether.Id == 0) continue;
            uint to = (uint)(ulong)tether.TargetId;
            if (to == 0 || to == 0xE0000000) continue;
            var key = (bc.EntityId, to);
            if (!seen.Add(key)) continue;
            DrawTetherLine(dl, toScreen, bc.EntityId, to);
        }
    }

    private static Vector2 WorldToScreen(float wx, float wz, Vector2 origin, float size)
        => new(
            origin.X + (wx - (CenterX - HalfExtent)) / (HalfExtent * 2f) * size,
            origin.Y + (wz - (CenterZ - HalfExtent)) / (HalfExtent * 2f) * size);
}
