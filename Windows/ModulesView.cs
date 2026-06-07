using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Utility;
using YapYapDraw.Engine.ModuleSetup;

namespace YapYapDraw.Windows;

public sealed class ModulesView
{
    private readonly Plugin _plugin;
    private Category _category = Category.Savage;
    private string _search = string.Empty;
    private readonly HashSet<string> _expanded = new();

    private static readonly (Category Cat, string Label, FontAwesomeIcon Icon)[] KnownCategories =
    {
        (Category.Dungeon,    "Dungeons",         FontAwesomeIcon.Dungeon),
        (Category.Trial,      "Trials",           FontAwesomeIcon.Dragon),
        (Category.Extreme,    "Extreme",          FontAwesomeIcon.Fire),
        (Category.Unreal,     "Unreal",           FontAwesomeIcon.Ghost),
        (Category.Raid,       "Raids",            FontAwesomeIcon.Users),
        (Category.Savage,     "Savage",           FontAwesomeIcon.Skull),
        (Category.Ultimate,   "Ultimate",         FontAwesomeIcon.Crown),
        (Category.Alliance,   "Alliance",         FontAwesomeIcon.UserFriends),
        (Category.Chaotic,    "Chaotic",          FontAwesomeIcon.Bolt),
        (Category.Foray,      "Field Operations", FontAwesomeIcon.MapMarkedAlt),
        (Category.DeepDungeon,"Deep Dungeon",     FontAwesomeIcon.LayerGroup),
        (Category.TreasureHunt,"Treasure Hunt",   FontAwesomeIcon.Gem),
        (Category.VariantCriterion, "Variant",    FontAwesomeIcon.Random),
    };

    // Keep the default mechanic order, only nudging a few into pull order.
    private static readonly Dictionary<string, (string Move, string After)[]> MechMoves = new()
    {
        ["Lindblum"] = new[]
        {
            ("Replication 2 (Clones + Bait)", "Double Kick"),
            ("Idyllic Dream (Uptime)", "Replication 2 (Clones + Bait)"),
        },
    };

    private static IEnumerable<Engine.FightModuleHost.MechView> OrderMechs(
        string fightKey, IEnumerable<Engine.FightModuleHost.MechView> mechs)
    {
        var list = mechs.ToList();
        if (!MechMoves.TryGetValue(fightKey, out var moves))
            return list;
        foreach (var (move, after) in moves)
        {
            int mi = list.FindIndex(m => m.Display == move);
            if (mi < 0) continue;
            var item = list[mi];
            list.RemoveAt(mi);
            int ai = list.FindIndex(m => m.Display == after);
            list.Insert(ai < 0 ? list.Count : ai + 1, item);
        }
        return list;
    }

    public ModulesView(Plugin plugin) => _plugin = plugin;

    public void Draw()
    {
        DrawCategoryPane();
        ImGui.SameLine();
        DrawFightPane();
    }

    private (string Label, FontAwesomeIcon Icon) Meta(Category cat)
    {
        foreach (var c in KnownCategories)
            if (c.Cat == cat) return (c.Label, c.Icon);
        return (cat.ToString(), FontAwesomeIcon.FolderOpen);
    }

    private void DrawCategoryPane()
    {
        var host = _plugin.Host;
        float w = 196f * ImGuiHelpers.GlobalScale;
        ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(8f, 7f));
        if (!ImGui.BeginChild("##cats", new Vector2(w, 0), true))
        {
            ImGui.EndChild();
            ImGui.PopStyleVar();
            return;
        }

        var allFights = host.Fights;
        var dl = ImGui.GetWindowDrawList();
        foreach (var (cat, label, icon) in KnownCategories)
        {
            int count = allFights.Count(f => f.Category == cat);
            bool sel = _category == cat;

            var p0 = ImGui.GetCursorScreenPos();
            float rowH = ImGui.GetFrameHeight();
            float availW = ImGui.GetContentRegionAvail().X;

            if (ImGui.Selectable($"##cat{cat}", sel, ImGuiSelectableFlags.None, new Vector2(availW, rowH)))
                _category = cat;

            if (sel)
                dl.AddRectFilled(p0, new Vector2(p0.X + 3f, p0.Y + rowH),
                    ImGui.ColorConvertFloat4ToU32(Ui.Accent), 2f);

            float ty = p0.Y + (rowH - ImGui.GetTextLineHeight()) * 0.5f;
            var iconCol = sel ? Ui.Accent : (count > 0 ? new Vector4(0.8f, 0.7f, 0.72f, 1f) : Ui.Dimmed);

            ImGui.PushFont(UiBuilder.IconFont);
            dl.AddText(new Vector2(p0.X + 10f, ty), ImGui.ColorConvertFloat4ToU32(iconCol), icon.ToIconString());
            ImGui.PopFont();

            var textCol = sel ? new Vector4(1f, 1f, 1f, 1f) : (count > 0 ? new Vector4(0.85f, 0.82f, 0.83f, 1f) : Ui.Dimmed);
            dl.AddText(new Vector2(p0.X + 34f, ty), ImGui.ColorConvertFloat4ToU32(textCol), label);

            if (count > 0)
            {
                string badge = count.ToString();
                float bw = ImGui.CalcTextSize(badge).X;
                dl.AddText(new Vector2(p0.X + availW - bw - 6f, ty),
                    ImGui.ColorConvertFloat4ToU32(sel ? Ui.Accent : Ui.Dimmed), badge);
            }
        }

        ImGui.EndChild();
        ImGui.PopStyleVar();
    }

    private void DrawFightPane()
    {
        var cfg  = _plugin.Configuration;
        var host = _plugin.Host;
        if (!ImGui.BeginChild("##fights", new Vector2(0, 0), false))
        {
            ImGui.EndChild();
            return;
        }

        var (label, icon) = Meta(_category);

        ImGui.SetWindowFontScale(1.25f);
        ImGui.PushFont(UiBuilder.IconFont);
        ImGui.TextColored(Ui.Accent, icon.ToIconString());
        ImGui.PopFont();
        ImGui.SameLine();
        ImGui.AlignTextToFramePadding();
        ImGui.TextColored(new Vector4(1f, 1f, 1f, 1f), label);
        ImGui.SetWindowFontScale(1f);

        var fights = host.Fights
            .Where(f => f.Category == _category)
            .OrderByDescending(f => f.Cfc)
            .ThenBy(f => f.Display, System.StringComparer.OrdinalIgnoreCase)
            .ToList();

        int totalMech = fights.Sum(f => f.Mechanics.Count);
        ImGui.SameLine();
        ImGui.AlignTextToFramePadding();
        ImGui.TextColored(Ui.Dimmed, $"   {fights.Count} fights \u00B7 {totalMech} mechanics");

        // search box, right-aligned
        float searchW = 220f * ImGuiHelpers.GlobalScale;
        float avail = ImGui.GetContentRegionAvail().X;
        if (avail > searchW + 40f)
        {
            ImGui.SameLine(ImGui.GetCursorPosX() + (avail - searchW));
            ImGui.SetNextItemWidth(searchW);
            ImGui.InputTextWithHint("##search", "Search fights…", ref _search, 64);
        }

        ImGui.Spacing();
        ImGui.Separator();
        ImGui.Spacing();

        if (!string.IsNullOrWhiteSpace(_search))
            fights = fights.Where(f => f.Display.Contains(_search, System.StringComparison.OrdinalIgnoreCase)).ToList();

        if (fights.Count == 0)
        {
            ImGui.Spacing();
            ImGui.TextColored(Ui.Dimmed, "  Nothing here yet.");
            ImGui.EndChild();
            return;
        }

        foreach (var f in fights)
            DrawFightCard(cfg, f);

        ImGui.EndChild();
    }

    private void DrawFightCard(Configuration cfg, Engine.FightModuleHost.FightView f)
    {
        ImGui.PushID(f.Key);

        bool on = !cfg.DisabledFights.Contains(f.Key);
        bool open = _expanded.Contains(f.Key);
        int enabledMech = f.Mechanics.Count(m => !cfg.DisabledMechanics.Contains(f.Key + "/" + m.Key));

        var dl = ImGui.GetWindowDrawList();
        var hp0 = ImGui.GetCursorScreenPos();
        float availW = ImGui.GetContentRegionAvail().X;
        float frame = ImGui.GetFrameHeight();
        float headH = frame + 12f;
        var hp1 = new Vector2(hp0.X + availW, hp0.Y + headH);

        bool hover = ImGui.IsMouseHoveringRect(hp0, hp1);

        Vector4 bg = on
            ? new Vector4(0.150f, 0.150f, 0.150f, 1f)
            : new Vector4(0.105f, 0.105f, 0.105f, 1f);
        if (hover) bg += new Vector4(0.03f, 0.03f, 0.03f, 0f);

        dl.AddRectFilled(hp0, hp1, ImGui.ColorConvertFloat4ToU32(bg), 7f);
        dl.AddRect(hp0, hp1,
            ImGui.ColorConvertFloat4ToU32(hover ? Ui.Accent with { W = 0.5f } : new Vector4(0.30f, 0.30f, 0.30f, 0.45f)),
            7f, ImDrawFlags.None, 1f);

        if (f.IsActive)
            dl.AddRectFilled(hp0, new Vector2(hp0.X + 3.5f, hp1.Y),
                ImGui.ColorConvertFloat4ToU32(Ui.Green), 2f);

        float centerY = hp0.Y + (headH - frame) * 0.5f;

        ImGui.SetCursorScreenPos(new Vector2(hp0.X + 12f, centerY));
        bool toggled = on;
        if (Ui.ToggleSwitch("##fen", ref toggled))
        {
            if (toggled) cfg.DisabledFights.Remove(f.Key);
            else         cfg.DisabledFights.Add(f.Key);
            cfg.Save();
            on = toggled;
        }

        ImGui.SameLine(0, 8f);
        ImGui.SetCursorPosY(ImGui.GetCursorPosY() + (frame - ImGui.GetFrameHeight()) * 0.5f);
        if (ImGui.ArrowButton("##exp", open ? ImGuiDir.Down : ImGuiDir.Right))
        {
            if (open) _expanded.Remove(f.Key); else _expanded.Add(f.Key);
            open = !open;
        }

        ImGui.SameLine(0, 10f);
        ImGui.AlignTextToFramePadding();
        ImGui.TextColored(on ? new Vector4(1f, 1f, 1f, 1f) : Ui.Dimmed, f.Display);
        ImGui.SameLine(0, 8f);
        if (f.Mechanics.Count > 0)
            ImGui.TextColored(Ui.Dimmed, $"({f.Mechanics.Count})");
        else if (f.UseAutoDraw)
            ImGui.TextColored(Ui.Dimmed, "(auto)");

        if (f.Mechanics.Any(m => m.HasConfig))
        {
            ImGui.SameLine(0, 8f);
            ImGui.PushFont(UiBuilder.IconFont);
            ImGui.TextColored(on ? Ui.Accent : Ui.Dimmed, FontAwesomeIcon.Cog.ToIconString());
            ImGui.PopFont();
            if (ImGui.IsItemHovered())
                ImGui.SetTooltip("Has configurable strats \u2014 expand and hit Configure.");
        }

        // right side: ratio + active pill, drawn directly
        float tY = hp0.Y + (headH - ImGui.GetTextLineHeight()) * 0.5f;
        float rightX = hp1.X - 12f;

        if (f.IsActive)
        {
            const string pill = "ACTIVE";
            float pw = ImGui.CalcTextSize(pill).X;
            float padX = 7f;
            var pb1 = new Vector2(rightX, tY + ImGui.GetTextLineHeight() * 0.5f + 9f);
            var pb0 = new Vector2(rightX - pw - padX * 2f, tY - 3f);
            dl.AddRectFilled(pb0, pb1, ImGui.ColorConvertFloat4ToU32(new Vector4(0.20f, 0.52f, 0.30f, 0.85f)), 4f);
            dl.AddText(new Vector2(pb0.X + padX, tY), ImGui.ColorConvertFloat4ToU32(new Vector4(1f, 1f, 1f, 1f)), pill);
            rightX = pb0.X - 10f;
        }

        if (f.Mechanics.Count > 0)
        {
            string ratio = $"{enabledMech}/{f.Mechanics.Count}";
            float rw = ImGui.CalcTextSize(ratio).X;
            var ratioCol = enabledMech == f.Mechanics.Count ? Ui.Dimmed
                : (enabledMech == 0 ? Ui.Red : Ui.Gold);
            dl.AddText(new Vector2(rightX - rw, tY), ImGui.ColorConvertFloat4ToU32(on ? ratioCol : Ui.Dimmed), ratio);
        }

        ImGui.SetCursorScreenPos(new Vector2(hp0.X, hp1.Y + 3f));

        if (open)
        {
            float lineX = hp0.X + 22f;
            float yStart = ImGui.GetCursorScreenPos().Y + 2f;

            if (!on)
            {
                ImGui.Indent(34f);
                ImGui.TextColored(Ui.Dimmed, "Module is off \u2014 nothing will draw.");
                ImGui.Unindent(34f);
            }
            else if (f.Mechanics.Count == 0 && f.UseAutoDraw)
            {
                ImGui.Indent(34f);
                ImGui.TextColored(Ui.Dimmed, "Auto-draw \u2014 telegraphs from game action data.");
                ImGui.Unindent(34f);
            }

            ImGui.Indent(34f);

            var phases = f.Mechanics.Select(m => m.Phase).Distinct().OrderBy(p => p).ToList();
            bool multiPhase = phases.Count > 1;
            int mi = 0;
            bool firstPhase = true;

            foreach (var phase in phases)
            {
                if (multiPhase)
                {
                    if (!firstPhase) ImGui.Dummy(new Vector2(0, 4f));
                    ImGui.PushFont(UiBuilder.IconFont);
                    ImGui.TextColored(Ui.Accent, FontAwesomeIcon.CircleNotch.ToIconString());
                    ImGui.PopFont();
                    ImGui.SameLine(0, 7f);
                    ImGui.AlignTextToFramePadding();
                    ImGui.TextColored(Ui.Accent, $"Phase {phase}");
                    ImGui.PushStyleColor(ImGuiCol.Separator, new Vector4(0.35f, 0.35f, 0.35f, 0.55f));
                    ImGui.Separator();
                    ImGui.PopStyleColor();
                    ImGui.Dummy(new Vector2(0, 2f));
                }
                firstPhase = false;

                foreach (var mech in OrderMechs(f.Key, f.Mechanics.Where(m => m.Phase == phase)))
                {
                    ImGui.PushID(mi++);
                    string mkey = f.Key + "/" + mech.Key;
                    bool men = ModuleConfig.IsEnabled(mkey);
                    if (Ui.ToggleSwitch("##men", ref men))
                        ModuleConfig.SetEnabled(mkey, men);
                    ImGui.SameLine(0, 8f);
                    ImGui.AlignTextToFramePadding();
                    ImGui.TextColored(men && on ? new Vector4(0.92f, 0.92f, 0.94f, 1f) : Ui.Dimmed, mech.Display);

                    if (mech.HasConfig && mech.DrawConfig != null)
                    {
                        ImGui.SameLine(0, 12f);
                        ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0.24f, 0.24f, 0.24f, 0.90f));
                        ImGui.PushStyleColor(ImGuiCol.ButtonHovered, Ui.Accent with { W = 0.55f });
                        ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, new Vector2(8f, 2f));
                        var dc = mech.DrawConfig;
                        if (ImGui.SmallButton("Configure"))
                            _plugin.OpenModuleConfig(mech.Display, dc);
                        ImGui.PopStyleVar();
                        ImGui.PopStyleColor(2);
                    }

                    ImGui.PopID();
                }
            }
            ImGui.Unindent(34f);

            float yEnd = ImGui.GetCursorScreenPos().Y - 2f;
            dl.AddLine(new Vector2(lineX, yStart), new Vector2(lineX, yEnd),
                ImGui.ColorConvertFloat4ToU32(new Vector4(0.32f, 0.32f, 0.32f, 0.7f)), 1.5f);

            ImGui.Spacing();
        }

        ImGui.Dummy(new Vector2(0, 5f));
        ImGui.PopID();
    }
}
