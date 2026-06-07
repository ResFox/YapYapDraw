using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Utility;
using YapYapDraw.QuickDraws;

namespace YapYapDraw.Windows;

public sealed class QuickDrawsView
{
    private readonly Plugin _plugin;
    private string _category = Configuration.QuickCategory;
    private string _status = "";
    private readonly HashSet<string> _expanded = new();
    private readonly Dictionary<uint, Dalamud.Interface.Textures.ISharedImmediateTexture> _iconCache = new();

    private static readonly (string Cat, FontAwesomeIcon Icon)[] KnownCategories =
    {
        (Configuration.QuickCategory, FontAwesomeIcon.Bolt),
        ("General",          FontAwesomeIcon.Star),
        ("Personal",         FontAwesomeIcon.User),
        ("Dungeons",         FontAwesomeIcon.Dungeon),
        ("Trials",           FontAwesomeIcon.Dragon),
        ("Extreme",          FontAwesomeIcon.Fire),
        ("Savage",           FontAwesomeIcon.Skull),
        ("Ultimate",         FontAwesomeIcon.Crown),
        ("Alliance",         FontAwesomeIcon.UserFriends),
        ("Field Operations", FontAwesomeIcon.MapMarkedAlt),
    };

    public QuickDrawsView(Plugin plugin) => _plugin = plugin;

    public void Draw()
    {
        var cfg = _plugin.Configuration;

        bool master = cfg.QuickDrawsEnabled;
        if (Ui.ToggleSwitch("##qdmaster", ref master)) { cfg.QuickDrawsEnabled = master; cfg.Save(); }
        ImGui.SameLine(0, 8f);
        ImGui.AlignTextToFramePadding();
        ImGui.TextColored(master ? new Vector4(1f, 1f, 1f, 1f) : Ui.Dimmed, "Quick draws enabled");
        ImGui.SameLine(0, 14f);
        ImGui.AlignTextToFramePadding();
        ImGui.TextDisabled("Right-click a Fight Log line to make one. Shapes show on the floor when the event fires.");

        ImGui.SameLine();
        float avail = ImGui.GetContentRegionAvail().X;
        float spacing = ImGui.GetStyle().ItemSpacing.X;
        float clearW = 110f * ImGuiHelpers.GlobalScale;
        float btnW = 110f * ImGuiHelpers.GlobalScale;
        float groupW = clearW + spacing + btnW;
        if (avail > groupW) ImGui.SameLine(ImGui.GetCursorPosX() + (avail - groupW));
        if (ImGui.Button("Clear shapes")) _plugin.Host.CleanVfx();
        if (ImGui.IsItemHovered())
            ImGui.SetTooltip("Wipe every shape currently drawn on the floor (same as /yyd cleanvfx).");
        ImGui.SameLine();
        if (ImGui.Button("Import pack")) ImportFromClipboard();

        if (!string.IsNullOrEmpty(_status))
            ImGui.TextDisabled(_status);

        ImGui.Separator();

        DrawCategoryPane();
        ImGui.SameLine();
        DrawPackPane();
    }

    private IEnumerable<(string Cat, FontAwesomeIcon Icon)> AllCategories()
    {
        var known = KnownCategories.Select(k => k.Cat).ToHashSet();
        var extra = _plugin.Configuration.QuickDrawModules
            .Select(m => m.Category)
            .Where(c => !string.IsNullOrEmpty(c) && !known.Contains(c))
            .Distinct()
            .Select(c => (c, FontAwesomeIcon.FolderOpen));
        return KnownCategories.Concat(extra);
    }

    private void DrawCategoryPane()
    {
        var cfg = _plugin.Configuration;
        float w = 196f * ImGuiHelpers.GlobalScale;
        ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(8f, 7f));
        if (!ImGui.BeginChild("##qdcats", new Vector2(w, 0), true))
        {
            ImGui.EndChild();
            ImGui.PopStyleVar();
            return;
        }

        var dl = ImGui.GetWindowDrawList();
        foreach (var (cat, icon) in AllCategories())
        {
            int count = cfg.QuickDrawModules.Count(m => m.Category == cat);
            bool sel = _category == cat;

            var p0 = ImGui.GetCursorScreenPos();
            float rowH = ImGui.GetFrameHeight();
            float availW = ImGui.GetContentRegionAvail().X;

            if (ImGui.Selectable($"##qdcat{cat}", sel, ImGuiSelectableFlags.None, new Vector2(availW, rowH)))
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
            dl.AddText(new Vector2(p0.X + 34f, ty), ImGui.ColorConvertFloat4ToU32(textCol), cat);

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

    private void DrawPackPane()
    {
        var cfg = _plugin.Configuration;
        if (!ImGui.BeginChild("##qdpacks", new Vector2(0, 0), false))
        {
            ImGui.EndChild();
            return;
        }

        var icon = AllCategories().FirstOrDefault(c => c.Cat == _category).Icon;
        if (icon == default) icon = FontAwesomeIcon.FolderOpen;

        ImGui.SetWindowFontScale(1.25f);
        ImGui.PushFont(UiBuilder.IconFont);
        ImGui.TextColored(Ui.Accent, icon.ToIconString());
        ImGui.PopFont();
        ImGui.SameLine();
        ImGui.AlignTextToFramePadding();
        ImGui.TextColored(new Vector4(1f, 1f, 1f, 1f), _category);
        ImGui.SetWindowFontScale(1f);

        var packs = cfg.QuickDrawModules.Where(m => m.Category == _category).ToList();
        int totalDraws = packs.Sum(p => p.Draws.Count);
        ImGui.SameLine();
        ImGui.AlignTextToFramePadding();
        ImGui.TextColored(Ui.Dimmed, $"   {packs.Count} packs \u00B7 {totalDraws} draws");

        float availTop = ImGui.GetContentRegionAvail().X;
        float newW = 96f * ImGuiHelpers.GlobalScale;
        if (availTop > newW + 40f)
        {
            ImGui.SameLine(ImGui.GetCursorPosX() + (availTop - newW));
            if (ImGui.Button("+ New pack", new Vector2(newW, 0)))
            {
                var np = new QuickDrawModule { Name = "New pack", Category = _category };
                cfg.QuickDrawModules.Add(np);
                _expanded.Add(np.Id);
                cfg.Save();
            }
        }

        ImGui.Spacing();
        ImGui.Separator();
        ImGui.Spacing();

        if (packs.Count == 0)
        {
            ImGui.Spacing();
            ImGui.TextColored(Ui.Dimmed, "  No packs here yet. Add one, or right-click a Fight Log line to make a quick draw.");
            ImGui.EndChild();
            return;
        }

        QuickDrawModule? removeModule = null;
        foreach (var m in packs)
            DrawPackCard(cfg, m, ref removeModule);

        if (removeModule != null) { cfg.QuickDrawModules.Remove(removeModule); cfg.Save(); }

        ImGui.EndChild();
    }

    private void DrawPackCard(Configuration cfg, QuickDrawModule m, ref QuickDrawModule? removeModule)
    {
        ImGui.PushID(m.Id);

        bool on = m.Enabled;
        bool open = _expanded.Contains(m.Id);
        int enabledDraws = m.Draws.Count(d => d.Enabled);

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

        float centerY = hp0.Y + (headH - frame) * 0.5f;

        ImGui.SetCursorScreenPos(new Vector2(hp0.X + 12f, centerY));
        bool toggled = on;
        if (Ui.ToggleSwitch("##pen", ref toggled)) { m.Enabled = toggled; cfg.Save(); on = toggled; }

        ImGui.SameLine(0, 8f);
        ImGui.SetCursorPosY(ImGui.GetCursorPosY() + (frame - ImGui.GetFrameHeight()) * 0.5f);
        if (ImGui.ArrowButton("##exp", open ? ImGuiDir.Down : ImGuiDir.Right))
        {
            if (open) _expanded.Remove(m.Id); else _expanded.Add(m.Id);
            open = !open;
        }

        ImGui.SameLine(0, 10f);
        ImGui.AlignTextToFramePadding();
        ImGui.TextColored(on ? new Vector4(1f, 1f, 1f, 1f) : Ui.Dimmed, m.Name);
        ImGui.SameLine(0, 8f);
        ImGui.TextColored(Ui.Dimmed, $"({m.Draws.Count})");

        // right side: enabled ratio
        float tY = hp0.Y + (headH - ImGui.GetTextLineHeight()) * 0.5f;
        float rightX = hp1.X - 12f;
        if (m.Draws.Count > 0)
        {
            string ratio = $"{enabledDraws}/{m.Draws.Count}";
            float rw = ImGui.CalcTextSize(ratio).X;
            var ratioCol = enabledDraws == m.Draws.Count ? Ui.Dimmed
                : (enabledDraws == 0 ? Ui.Red : Ui.Gold);
            dl.AddText(new Vector2(rightX - rw, tY), ImGui.ColorConvertFloat4ToU32(on ? ratioCol : Ui.Dimmed), ratio);
        }

        ImGui.SetCursorScreenPos(new Vector2(hp0.X, hp1.Y + 3f));

        if (open)
        {
            ImGui.Indent(34f);
            DrawPackToolbar(m, ref removeModule);
            DrawPackDraws(m);
            ImGui.Unindent(34f);
            ImGui.Spacing();
        }

        ImGui.Dummy(new Vector2(0, 5f));
        ImGui.PopID();
    }

    private void DrawPackToolbar(QuickDrawModule m, ref QuickDrawModule? removeModule)
    {
        var cfg = _plugin.Configuration;

        ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0.24f, 0.24f, 0.24f, 0.90f));
        ImGui.PushStyleColor(ImGuiCol.ButtonHovered, Ui.Accent with { W = 0.55f });

        if (ImGui.SmallButton("+ Draw"))
        {
            var t = new QuickDrawDef();
            m.Draws.Add(t);
            cfg.Save();
            _plugin.OpenQuickDraw(t);
        }
        ImGui.SameLine();
        if (ImGui.SmallButton("Copy (share)")) ExportToClipboard(m);

        ImGui.PopStyleColor(2);

        ImGui.SameLine();
        ImGui.SetNextItemWidth(180f * ImGuiHelpers.GlobalScale);
        string name = m.Name;
        if (ImGui.InputText("##mname", ref name, 64)) { m.Name = name; cfg.Save(); }
        ImGui.SameLine();
        if (ImGui.SmallButton("Delete pack")) removeModule = m;

        ImGui.Spacing();
    }

    private void DrawPackDraws(QuickDrawModule m)
    {
        var cfg = _plugin.Configuration;

        QuickDrawDef? removeDraw = null;
        (QuickDrawDef t, QuickDrawModule dest)? move = null;
        string? lastGroup = null;

        foreach (var t in m.Draws.OrderBy(x => x.Group, StringComparer.OrdinalIgnoreCase))
        {
            if (!string.IsNullOrEmpty(t.Group) && t.Group != lastGroup)
            {
                ImGui.Spacing();
                ImGui.TextColored(Ui.Gold, t.Group);
                lastGroup = t.Group;
            }

            ImGui.PushID(t.Id);

            bool en = t.Enabled;
            if (Ui.ToggleSwitch("##ten", ref en)) { t.Enabled = en; cfg.Save(); }

            ImGui.SameLine(0, 8f);
            DrawIcon(t.IconId, ImGui.GetFrameHeight() * 0.9f);

            ImGui.SameLine();
            ImGui.AlignTextToFramePadding();
            ImGui.TextColored(en ? new Vector4(0.92f, 0.92f, 0.94f, 1f) : Ui.Dimmed, t.Name);
            ImGui.SameLine();
            DrawShapeChip(t.Draw);
            ImGui.SameLine();
            ImGui.TextColored(Ui.Dimmed, Summary(t));

            float avail = ImGui.GetContentRegionAvail().X;
            float btnW  = 196f * ImGuiHelpers.GlobalScale;
            if (avail > btnW) ImGui.SameLine(ImGui.GetCursorPosX() + (avail - btnW));
            else ImGui.SameLine();
            if (ImGui.SmallButton("Edit")) _plugin.OpenQuickDraw(t);
            ImGui.SameLine();
            if (ImGui.SmallButton("Test")) _plugin.Engine.Preview(t);
            ImGui.SameLine();
            if (ImGui.SmallButton("Move")) ImGui.OpenPopup($"move{t.Id}");
            ImGui.SameLine();
            if (ImGui.SmallButton("X")) removeDraw = t;

            if (ImGui.BeginPopup($"move{t.Id}"))
            {
                ImGui.TextDisabled("Move to pack");
                ImGui.Separator();
                foreach (var dest in cfg.QuickDrawModules.Where(x => x != m))
                    if (ImGui.MenuItem($"{dest.Name}  ({dest.Category})"))
                        move = (t, dest);

                ImGui.Separator();
                if (ImGui.MenuItem("+ New pack here…"))
                {
                    var dest = new QuickDrawModule { Name = "New pack", Category = _category };
                    cfg.QuickDrawModules.Add(dest);
                    move = (t, dest);
                }
                ImGui.EndPopup();
            }

            ImGui.PopID();
        }

        if (removeDraw != null) { m.Draws.Remove(removeDraw); cfg.Save(); }
        if (move is { } mv) { m.Draws.Remove(mv.t); mv.dest.Draws.Add(mv.t); cfg.Save(); }
    }

    private static void DrawShapeChip(DrawSpec d)
    {
        var dl = ImGui.GetWindowDrawList();
        var p = ImGui.GetCursorScreenPos();
        float h = ImGui.GetTextLineHeight();
        var c = ImGui.ColorConvertFloat4ToU32(d.Color with { W = 1f });
        var mid = new Vector2(p.X + h * 0.5f, p.Y + h * 0.5f);
        switch (d.Shape)
        {
            case QuickShape.Circle:
                dl.AddCircle(mid, h * 0.45f, c, 16, 2f);
                break;
            case QuickShape.Donut:
                dl.AddCircle(mid, h * 0.45f, c, 16, 2f);
                dl.AddCircle(mid, h * 0.22f, c, 12, 2f);
                break;
            case QuickShape.Fan:
                dl.PathArcTo(mid, h * 0.45f, -2.4f, -0.7f, 12);
                dl.PathLineTo(mid);
                dl.PathStroke(c, ImDrawFlags.Closed, 2f);
                break;
            case QuickShape.Rectangle:
                dl.AddRect(new Vector2(p.X + 1f, p.Y + 2f), new Vector2(p.X + h - 1f, p.Y + h - 2f), c, 1f, ImDrawFlags.None, 2f);
                break;
            case QuickShape.Line:
                dl.AddLine(new Vector2(p.X + 2f, p.Y + h - 2f), new Vector2(p.X + h - 2f, p.Y + 2f), c, 2f);
                break;
            case QuickShape.Tower:
                dl.AddCircle(mid, h * 0.42f, c, 16, 2f);
                dl.AddCircleFilled(mid, h * 0.16f, c);
                break;
            case QuickShape.Knockback:
                dl.AddTriangleFilled(
                    new Vector2(mid.X, p.Y + 1f),
                    new Vector2(p.X + 1f, p.Y + h - 1f),
                    new Vector2(p.X + h - 1f, p.Y + h - 1f), c);
                break;
            case QuickShape.Laser:
                dl.AddRectFilled(new Vector2(p.X + h * 0.35f, p.Y + 2f), new Vector2(p.X + h - 1f, p.Y + h - 2f), c, 1f);
                dl.AddTriangleFilled(
                    new Vector2(p.X + 1f, p.Y + h * 0.5f),
                    new Vector2(p.X + h * 0.35f, p.Y + 2f),
                    new Vector2(p.X + h * 0.35f, p.Y + h - 2f), c);
                break;
        }
        ImGui.Dummy(new Vector2(h, h));
    }

    private void DrawIcon(uint iconId, float size)
    {
        if (iconId == 0) { ImGui.Dummy(new Vector2(size, size)); return; }
        if (!_iconCache.TryGetValue(iconId, out var tex))
        {
            if (_iconCache.Count > 256) _iconCache.Clear();
            tex = Plugin.TextureProvider.GetFromGameIcon(new Dalamud.Interface.Textures.GameIconLookup(iconId));
            _iconCache[iconId] = tex;
        }
        var wrap = tex?.GetWrapOrDefault();
        if (wrap != null) ImGui.Image(wrap.Handle, new Vector2(size, size));
        else ImGui.Dummy(new Vector2(size, size));
    }

    private static string Summary(QuickDrawDef t)
    {
        string what = t.MatchById ? $"#{t.DataId}" : (string.IsNullOrEmpty(t.Pattern) ? "any" : t.Pattern);
        return $"[{t.On}: {what}]";
    }

    private void ExportToClipboard(QuickDrawModule m)
    {
        try
        {
            var clone = new QuickDrawModule
            {
                Name = m.Name, Category = m.Category, Author = m.Author,
                Draws = m.Draws.Select(x => x.Clone()).ToList(),
            };
            ImGui.SetClipboardText(ShareCodec.Encode(ShareCodec.ModulePrefix, clone));
            _status = "Share code copied";
        }
        catch (Exception ex) { _status = "Copy failed"; Plugin.Log.Warning($"[YapYapDraw] export: {ex.Message}"); }
    }

    private void ImportFromClipboard()
    {
        var code = ImGui.GetClipboardText();
        if (string.IsNullOrWhiteSpace(code)) { _status = "Clipboard empty"; return; }

        if (ShareCodec.TryDecode<QuickDrawModule>(ShareCodec.ModulePrefix, code, out var mod)
            && mod is { Draws: not null })
        {
            mod.Id      = Guid.NewGuid().ToString("N");
            mod.BuiltIn = false;
            foreach (var t in mod.Draws) t.Id = Guid.NewGuid().ToString("N");
            if (string.IsNullOrWhiteSpace(mod.Category)) mod.Category = "General";

            _plugin.Configuration.QuickDrawModules.Add(mod);
            _plugin.Configuration.Save();
            _category = mod.Category;
            _status = $"Imported \"{mod.Name}\"";
            return;
        }

        if (ShareCodec.TryDecode<QuickDrawDef>(ShareCodec.DrawPrefix, code, out var trig) && trig != null)
        {
            trig.Id = Guid.NewGuid().ToString("N");
            var target = _plugin.Configuration.QuickDrawModules
                .Find(x => x.Category == _category) ?? _plugin.Configuration.QuickModule();
            target.Draws.Add(trig);
            _plugin.Configuration.Save();
            _status = $"Imported draw \"{trig.Name}\"";
            return;
        }

        _status = "Not a YapYapDraw code";
    }
}
