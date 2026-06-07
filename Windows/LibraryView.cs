using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility;
using YapYapDraw.QuickDraws;

namespace YapYapDraw.Windows;

// Browser over the learned fight catalog.
public sealed class LibraryView
{
    private readonly Plugin _plugin;
    private uint   _zone;
    private string _zoneSearch  = "";
    private string _entrySearch = "";
    private bool   _showCasts   = true;
    private bool   _showStatus  = true;
    private bool   _showMarkers = true;
    private bool   _showTethers = true;
    private readonly Dictionary<uint, Dalamud.Interface.Textures.ISharedImmediateTexture> _iconCache = new();

    public LibraryView(Plugin plugin) => _plugin = plugin;

    public void Draw()
    {
        var cat = _plugin.Catalog;

        ImGui.TextDisabled("Fights fill in as you play or replay them. Pick a duty, then turn any cast or debuff into a quick draw.");
        ImGui.Separator();

        DrawZonePane(cat);
        ImGui.SameLine();
        DrawEntryPane(cat);
    }

    private void DrawZonePane(FightCatalog cat)
    {
        float scale = ImGuiHelpers.GlobalScale;
        if (!ImGui.BeginChild("##libzones", new Vector2(240f * scale, 0), true)) { ImGui.EndChild(); return; }

        ImGui.SetNextItemWidth(-1);
        ImGui.InputTextWithHint("##zq", "filter duties…", ref _zoneSearch, 64);

        var zones = cat.Zones();
        if (zones.Count == 0)
            ImGui.TextWrapped("Nothing recorded yet. Run or replay a duty and it'll show up here.");

        var grouped = zones
            .Select(t => (Terr: t, Name: ZoneLibrary.NameOf(t), Cat: ZoneLibrary.CategoryOf(t)))
            .Where(z => string.IsNullOrWhiteSpace(_zoneSearch) ||
                        z.Name.Contains(_zoneSearch, StringComparison.OrdinalIgnoreCase))
            .GroupBy(z => z.Cat)
            .OrderBy(g => g.Key, StringComparer.OrdinalIgnoreCase);

        foreach (var group in grouped)
        {
            if (!ImGui.CollapsingHeader($"{group.Key}###cat{group.Key}", ImGuiTreeNodeFlags.DefaultOpen))
                continue;

            foreach (var z in group.OrderBy(z => z.Name, StringComparer.OrdinalIgnoreCase))
            {
                if (ImGui.Selectable($"{z.Name}  ({cat.Count(z.Terr)})##z{z.Terr}", _zone == z.Terr))
                    _zone = z.Terr;
            }
        }

        ImGui.EndChild();
    }

    private void DrawEntryPane(FightCatalog cat)
    {
        if (!ImGui.BeginChild("##libentries", new Vector2(0, 0), false)) { ImGui.EndChild(); return; }

        if (_zone == 0)
        {
            ImGui.TextDisabled("Select a duty on the left.");
            ImGui.EndChild();
            return;
        }

        ImGui.AlignTextToFramePadding();
        ImGui.TextColored(Ui.Gold, ZoneLibrary.NameOf(_zone));
        ImGui.SameLine();
        if (ImGui.SmallButton("Clear this duty")) { cat.Clear(_zone); }

        ImGui.SetNextItemWidth(260f * ImGuiHelpers.GlobalScale);
        ImGui.InputTextWithHint("##eq", "search name or id…", ref _entrySearch, 64);
        ImGui.SameLine();
        ImGui.Checkbox("Casts", ref _showCasts);   ImGui.SameLine();
        ImGui.Checkbox("Statuses", ref _showStatus); ImGui.SameLine();
        ImGui.Checkbox("Markers", ref _showMarkers); ImGui.SameLine();
        ImGui.Checkbox("Tethers", ref _showTethers);

        var entries = cat.Entries(_zone)
            .Where(e => string.IsNullOrWhiteSpace(_entrySearch) ||
                        e.Name.Contains(_entrySearch, StringComparison.OrdinalIgnoreCase) ||
                        e.Id.ToString().Contains(_entrySearch))
            .ToList();

        if (_showCasts)   DrawGroup("Casts",       entries, FightCatalog.Kind.Cast);
        if (_showStatus)  DrawGroup("Statuses",    entries, FightCatalog.Kind.Status);
        if (_showMarkers) DrawGroup("Headmarkers", entries, FightCatalog.Kind.Headmarker);
        if (_showTethers) DrawGroup("Tethers",     entries, FightCatalog.Kind.Tether);

        ImGui.EndChild();
    }

    private void DrawGroup(string label, List<FightCatalog.Entry> all, FightCatalog.Kind kind)
    {
        var rows = all.Where(e => e.Kind == kind)
                      .OrderBy(e => e.Name, StringComparer.OrdinalIgnoreCase)
                      .ToList();
        if (rows.Count == 0) return;

        if (!ImGui.CollapsingHeader($"{label}  ({rows.Count})###grp{label}", ImGuiTreeNodeFlags.DefaultOpen))
            return;

        foreach (var e in rows)
        {
            ImGui.PushID($"{label}{e.Id}");
            DrawIcon(e.Icon, ImGui.GetFrameHeight() * 0.9f);
            ImGui.SameLine();
            ImGui.AlignTextToFramePadding();
            ImGui.TextUnformatted(e.Name);
            ImGui.SameLine();
            ImGui.TextColored(Ui.Dimmed, $"#{e.Id}");

            float avail = ImGui.GetContentRegionAvail().X;
            float btnW  = 190f * ImGuiHelpers.GlobalScale;
            if (avail > btnW) ImGui.SameLine(ImGui.GetCursorPosX() + (avail - btnW));
            else ImGui.SameLine();

            if (ImGui.SmallButton("Make quick draw")) _plugin.OpenQuickDrawForCatalog(e, _zone);
            ImGui.SameLine();
            if (ImGui.SmallButton("Copy id")) ImGui.SetClipboardText(e.Id.ToString());

            ImGui.PopID();
        }
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
}
