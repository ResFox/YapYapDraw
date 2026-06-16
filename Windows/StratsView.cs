using System;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility;
using YapYapDraw.QuickDraws;
using YapYapDraw.Strats;

namespace YapYapDraw.Windows;

public sealed class StratsView
{
    private readonly Plugin _plugin;
    private string _status = "";
    private static readonly string[] RoleNames = { "MT", "OT", "M1", "M2", "R1", "R2", "H1", "H2" };

    public StratsView(Plugin plugin) => _plugin = plugin;

    public void Draw()
    {
        var cfg   = _plugin.Configuration;
        float scale = ImGuiHelpers.GlobalScale;

        bool master = cfg.StratsEnabled;
        if (Ui.ToggleSwitch("##stratmaster", ref master)) { cfg.StratsEnabled = master; cfg.Save(); }
        ImGui.SameLine(0, 8f);
        ImGui.AlignTextToFramePadding();
        ImGui.TextColored(master ? new Vector4(1f, 1f, 1f, 1f) : Ui.Dimmed, "Strats enabled");

        ImGui.SameLine(0, 16f);
        ImGui.AlignTextToFramePadding();
        ImGui.TextColored(Ui.Dimmed, "Your role");
        ImGui.SameLine();
        ImGui.SetNextItemWidth(80f * scale);
        int role = (int)cfg.MyRole;
        if (ImGui.Combo("##myrole", ref role, RoleNames, RoleNames.Length)) { cfg.MyRole = (StratRole)role; cfg.Save(); }

        ImGui.SameLine(0, 16f);
        ImGui.AlignTextToFramePadding();
        ImGui.TextDisabled("Pick a strat for this zone and your role; only your spot draws in the fight.");

        ImGui.Separator();

        uint terr = Plugin.ClientState.TerritoryType;
        ImGui.AlignTextToFramePadding();
        ImGui.TextColored(Ui.Dimmed, $"Current zone: {terr}");
        ImGui.SameLine();
        if (ImGui.Button("+ New strat (this zone)"))
        {
            var p = new StratPack { Name = "New strat", Territory = terr };
            cfg.StratPacks.Add(p);
            cfg.Save();
            _plugin.OpenStrat(p);
        }
        ImGui.SameLine();
        if (ImGui.Button("+ Example (Idyllic)"))
        {
            var ex = StratLibrary.BuildIdyllicExample(terr);
            cfg.StratPacks.Add(ex);
            cfg.SelectedStrat[terr.ToString()] = ex.Id;
            cfg.Save();
            _plugin.OpenStrat(ex);
        }
        ImGui.SameLine();
        if (ImGui.Button("Clear shapes")) _plugin.Host.CleanVfx();
        ImGui.SameLine();
        if (ImGui.Button("Import strat")) ImportFromClipboard();

        if (!string.IsNullOrEmpty(_status))
        {
            ImGui.SameLine();
            ImGui.TextDisabled(_status);
        }

        ImGui.Spacing();
        ImGui.Separator();
        ImGui.Spacing();

        if (cfg.StratPacks.Count == 0)
        {
            ImGui.TextColored(Ui.Dimmed, "No strats yet. Make one for this zone and place your role's spots.");
            return;
        }

        StratPack? remove = null;
        foreach (var p in cfg.StratPacks)
        {
            ImGui.PushID(p.Id);

            bool en = p.Enabled;
            if (Ui.ToggleSwitch("##pen", ref en)) { p.Enabled = en; cfg.Save(); }

            ImGui.SameLine(0, 8f);
            bool active = IsActive(cfg, p);
            ImGui.AlignTextToFramePadding();
            var nameCol = active ? Ui.Green : (p.Territory == terr ? new Vector4(1f, 1f, 1f, 1f) : Ui.Dimmed);
            ImGui.TextColored(nameCol, p.Name);
            ImGui.SameLine();
            ImGui.TextColored(Ui.Dimmed, $"(zone {p.Territory} \u00B7 {p.Slides.Count} steps)");

            float avail = ImGui.GetContentRegionAvail().X;
            float grp   = 220f * scale;
            if (avail > grp) ImGui.SameLine(ImGui.GetCursorPosX() + (avail - grp));
            else ImGui.SameLine();

            if (ImGui.SmallButton(active ? "Active" : "Select"))
            { cfg.SelectedStrat[p.Territory.ToString()] = p.Id; cfg.Save(); }
            ImGui.SameLine();
            if (ImGui.SmallButton("Edit")) _plugin.OpenStrat(p);
            ImGui.SameLine();
            if (ImGui.SmallButton("Share")) ExportToClipboard(p);
            ImGui.SameLine();
            if (ImGui.SmallButton("X")) remove = p;

            if (active) DrawManualPicks(p);

            ImGui.PopID();
            ImGui.Spacing();
        }

        if (remove != null) { cfg.StratPacks.Remove(remove); cfg.Save(); }
    }

    private void DrawManualPicks(StratPack p)
    {
        ImGui.Indent(20f);
        foreach (var s in p.Slides)
        {
            if (s.Branches.Count <= 1) continue;
            bool anyManual = s.Branches.Exists(b => b.Detect == BranchDetect.Manual);
            if (!anyManual) continue;

            ImGui.AlignTextToFramePadding();
            ImGui.TextColored(Ui.Dimmed, $"{s.Name}:");
            ImGui.SameLine();

            var cur = _plugin.Strat.GetManualBranch(s.Id);
            foreach (var b in s.Branches)
            {
                bool sel = cur == b.Id;
                if (sel) ImGui.PushStyleColor(ImGuiCol.Button, Ui.Accent with { W = 0.85f });
                if (ImGui.SmallButton($"{b.Name}##{b.Id}")) _plugin.Strat.SetManualBranch(s.Id, b.Id);
                if (sel) ImGui.PopStyleColor();
                ImGui.SameLine();
            }
            ImGui.NewLine();
        }
        ImGui.Unindent(20f);
    }

    private void ExportToClipboard(StratPack p)
    {
        try
        {
            var clone = p.Clone();
            clone.BuiltIn = false;
            ImGui.SetClipboardText(ShareCodec.Encode(ShareCodec.StratPrefix, clone));
            _status = "Strat code copied";
        }
        catch (Exception ex) { _status = "Copy failed"; Plugin.Log.Warning($"[YapYapDraw] strat export: {ex.Message}"); }
    }

    private void ImportFromClipboard()
    {
        var code = ImGui.GetClipboardText();
        if (string.IsNullOrWhiteSpace(code)) { _status = "Clipboard empty"; return; }

        if (ShareCodec.TryDecode<StratPack>(ShareCodec.StratPrefix, code, out var pack) && pack is { Slides: not null })
        {
            pack.Id      = Guid.NewGuid().ToString("N");
            pack.BuiltIn = false;
            foreach (var s in pack.Slides)
            {
                s.Id = Guid.NewGuid().ToString("N");
                foreach (var b in s.Branches) b.Id = Guid.NewGuid().ToString("N");
            }
            _plugin.Configuration.StratPacks.Add(pack);
            _plugin.Configuration.Save();
            _status = $"Imported \"{pack.Name}\"";
            return;
        }

        _status = "Not a YapYap strat code";
    }

    private static bool IsActive(Configuration cfg, StratPack p)
    {
        if (!cfg.StratsEnabled || !p.Enabled) return false;
        if (p.Territory != Plugin.ClientState.TerritoryType) return false;

        if (cfg.SelectedStrat.TryGetValue(p.Territory.ToString(), out var id) && !string.IsNullOrEmpty(id))
            return id == p.Id;

        return cfg.StratPacks.Find(x => x.Enabled && x.Territory == p.Territory) == p;
    }
}
