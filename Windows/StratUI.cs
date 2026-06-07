using System;
using System.Collections.Generic;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;

namespace YapYapDraw.Windows;

public static class StratUI
{
    public static readonly (string Name, Vector4 Col)[] Swatches =
    {
        ("White",  new Vector4(1.00f, 1.00f, 1.00f, 1f)),
        ("Orange", new Vector4(0.949f, 0.310f, 0.075f, 1f)),
        ("Blue",   new Vector4(0.27f, 0.55f, 1.00f, 1f)),
        ("Red",    new Vector4(0.95f, 0.25f, 0.25f, 1f)),
        ("Green",  new Vector4(0.20f, 0.90f, 0.35f, 1f)),
        ("Yellow", new Vector4(1.00f, 0.85f, 0.15f, 1f)),
        ("Cyan",   new Vector4(0.20f, 0.85f, 0.90f, 1f)),
        ("Purple", new Vector4(0.70f, 0.30f, 1.00f, 1f)),
    };

    public static Vector4 SwatchColor(int index)
        => index >= 0 && index < Swatches.Length ? Swatches[index].Col : Swatches[0].Col;

    public static string SwatchName(int index)
        => index >= 0 && index < Swatches.Length ? Swatches[index].Name : Swatches[0].Name;

    public static bool Header(string title, ref bool active)
    {
        ImGui.SetWindowFontScale(1.18f);
        ImGui.AlignTextToFramePadding();
        ImGui.TextColored(Ui.Accent, title);
        ImGui.SetWindowFontScale(1f);

        float toggleW = ImGui.GetFrameHeight() * 1.8f;
        string label = active ? "ACTIVE" : "OFF";
        float labelW = ImGui.CalcTextSize(label).X;
        float pad = ImGui.GetStyle().ItemSpacing.X;
        ImGui.SameLine();
        ImGui.SetCursorPosX(ImGui.GetWindowWidth() - toggleW - labelW - pad - 14f);
        ImGui.AlignTextToFramePadding();
        ImGui.TextColored(active ? Ui.Accent : Ui.Dimmed, label);
        ImGui.SameLine();
        bool changed = Ui.ToggleSwitch("##active_" + title, ref active);

        ImGui.Separator();
        ImGui.Spacing();
        return changed;
    }

    public static void Section(string label)
    {
        ImGui.Spacing();
        ImGui.PushStyleColor(ImGuiCol.Text, Ui.Accent);
        ImGui.TextUnformatted(label.ToUpperInvariant());
        ImGui.PopStyleColor();
    }

    public static void Hint(string text)
        => ImGui.TextDisabled(text);

    public static bool SegmentedBar(string[] options, ref int selected)
    {
        bool changed = false;
        for (int i = 0; i < options.Length; i++)
        {
            bool sel = selected == i;
            if (sel)
            {
                ImGui.PushStyleColor(ImGuiCol.Button, Ui.Accent);
                ImGui.PushStyleColor(ImGuiCol.ButtonHovered, Ui.Accent with { W = 0.9f });
                ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(0.08f, 0.06f, 0.05f, 1f));
            }
            if (ImGui.Button(options[i]))
            {
                if (selected != i) changed = true;
                selected = i;
            }
            if (sel) ImGui.PopStyleColor(3);
            if (i < options.Length - 1) ImGui.SameLine();
        }
        return changed;
    }

    public static bool RoleGrid(string[] roles, ref int selected, int columns = 2)
    {
        bool changed = false;
        if (!ImGui.BeginTable("##rolegrid", columns, ImGuiTableFlags.SizingStretchSame))
        {
            return false;
        }
        for (int i = 0; i < roles.Length; i++)
        {
            ImGui.TableNextColumn();
            bool sel = selected == i;
            if (sel)
            {
                ImGui.PushStyleColor(ImGuiCol.Button, Ui.Accent);
                ImGui.PushStyleColor(ImGuiCol.ButtonHovered, Ui.Accent with { W = 0.9f });
                ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(0.08f, 0.06f, 0.05f, 1f));
            }
            if (ImGui.Button(roles[i], new Vector2(-1f, 0f)))
            {
                if (selected != i) changed = true;
                selected = i;
            }
            if (sel) ImGui.PopStyleColor(3);
        }
        ImGui.EndTable();
        return changed;
    }

    public static bool ColorSwatches(ref int index)
    {
        bool changed = false;
        float sq = ImGui.GetFrameHeight();
        var draw = ImGui.GetWindowDrawList();
        for (int i = 0; i < Swatches.Length; i++)
        {
            if (ImGui.ColorButton($"##sw{i}", Swatches[i].Col, ImGuiColorEditFlags.NoTooltip | ImGuiColorEditFlags.NoDragDrop, new Vector2(sq, sq)))
            {
                if (index != i) changed = true;
                index = i;
            }
            if (ImGui.IsItemHovered()) ImGui.SetTooltip(Swatches[i].Name);
            if (index == i)
            {
                Vector2 a = ImGui.GetItemRectMin();
                Vector2 b = ImGui.GetItemRectMax();
                draw.AddRect(a - new Vector2(2f, 2f), b + new Vector2(2f, 2f),
                    ImGui.ColorConvertFloat4ToU32(new Vector4(1f, 1f, 1f, 1f)), 4f, ImDrawFlags.None, 2f);
            }
            if (i < Swatches.Length - 1) ImGui.SameLine();
        }
        return changed;
    }

    // top of the list = highest priority
    public static bool PriorityList(string id, List<string> items)
    {
        bool changed = false;
        float btn = ImGui.GetFrameHeight();
        float spacing = ImGui.GetStyle().ItemSpacing.X;
        float rightX = ImGui.GetWindowWidth() - btn * 2f - spacing - 16f;
        for (int i = 0; i < items.Count; i++)
        {
            ImGui.PushID($"{id}_{i}");
            ImGui.AlignTextToFramePadding();
            ImGui.TextColored(Ui.Dimmed, $"{i + 1}.");
            ImGui.SameLine();
            ImGui.TextUnformatted(items[i]);

            ImGui.SameLine(rightX);
            ImGui.PushFont(UiBuilder.IconFont);
            bool up = ImGui.Button(FontAwesomeIcon.ChevronUp.ToIconString() + "##u", new Vector2(btn, btn));
            ImGui.SameLine();
            bool down = ImGui.Button(FontAwesomeIcon.ChevronDown.ToIconString() + "##d", new Vector2(btn, btn));
            ImGui.PopFont();

            if (up && i > 0)
            {
                (items[i - 1], items[i]) = (items[i], items[i - 1]);
                changed = true;
            }
            if (down && i < items.Count - 1)
            {
                (items[i + 1], items[i]) = (items[i], items[i + 1]);
                changed = true;
            }
            ImGui.PopID();
        }
        return changed;
    }
}
