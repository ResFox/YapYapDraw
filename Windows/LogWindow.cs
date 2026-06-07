using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.Json;
using Dalamud.Bindings.ImGui;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Interface.Textures;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Windowing;
using YapYapDraw.Logging;

namespace YapYapDraw.Windows;

public sealed class LogWindow : Window, IDisposable
{
    private readonly Plugin _plugin;
    private readonly Dictionary<uint, ISharedImmediateTexture> _iconCache = new();
    private string _search = "";
    private int    _pullFilter;
    private string _exportStatus = "";

    private enum SearchScope { Any, Source, Target, Ability }
    private static readonly string[] SearchScopeNames = { "any", "source", "target", "ability" };
    private SearchScope _searchScope = SearchScope.Any;

    // In-memory follow filter: when set, only events touching this entity show.
    private uint   _focusId;
    private string _focusName = "";

    // View controls. Freeze pins the table so streaming events don't move it;
    // auto-scroll snaps back to the newest row as events arrive.
    private bool _paused;
    private bool _autoScroll = true;
    private bool _scrollToLatest;
    private int  _prevFilteredCount = -1;

    private static readonly Vector4 ColCast   = new(1f, 0.55f, 0.30f, 1f);
    private static readonly Vector4 ColUse     = new(0.95f, 0.80f, 0.45f, 1f);
    private static readonly Vector4 ColGain   = new(0.55f, 0.85f, 1f, 1f);
    private static readonly Vector4 ColLose   = new(0.55f, 0.55f, 0.60f, 1f);
    private static readonly Vector4 ColDeath  = new(1f, 0.35f, 0.35f, 1f);
    private static readonly Vector4 ColMarker = new(0.85f, 0.55f, 1f, 1f);
    private static readonly Vector4 ColDim    = new(0.65f, 0.65f, 0.65f, 1f);
    private static readonly Vector4 ColEnemy  = new(1f, 0.45f, 0.42f, 1f);
    private static readonly Vector4 ColYou    = new(0.55f, 0.85f, 1f, 1f);
    private static readonly Vector4 ColParty  = new(0.55f, 0.90f, 0.60f, 1f);
    private static readonly Vector4 ColId     = new(0.60f, 0.70f, 0.85f, 1f);
    private static readonly Vector4 ColMap    = new(0.45f, 0.90f, 0.80f, 1f);
    private static readonly Vector4 ColCtrl   = new(0.70f, 0.70f, 0.78f, 1f);
    private static readonly Vector4 ColNote   = new(1f, 0.85f, 0.40f, 1f);

    public LogWindow(Plugin plugin)
        : base("YapYap Fight Log###YapYapLog")
    {
        _plugin = plugin;
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(420, 250),
            MaximumSize = new Vector2(2000, 2000),
        };
    }

    public void Dispose() { }

    public override void OnClose()
    {
        if (_plugin.Configuration.LogWindowOpen)
        {
            _plugin.Configuration.LogWindowOpen = false;
            _plugin.Configuration.Save();
        }
    }

    public override void Draw() => DrawContent();

    private static readonly string[] CaptureNames = { "Always (everything)", "Only in combat", "Only in a duty" };

    public void DrawContent()
    {
        var cfg = _plugin.Configuration;

        DrawZoneBar();
        DrawCaptureRow(cfg);
        DrawKindToggles(cfg);
        DrawAuthorToggles(cfg);
        DrawSearchRow();
        DrawFocusBanner();

        ImGui.Separator();

        DrawPullSidebar();
        ImGui.SameLine();
        DrawTable();
    }

    private void DrawCaptureRow(Configuration cfg)
    {
        ImGui.AlignTextToFramePadding();
        ImGui.TextDisabled("Capture:");
        ImGui.SameLine();
        int cap = (int)cfg.CaptureWhen;
        ImGui.SetNextItemWidth(170f * ImGuiHelpers.GlobalScale);
        if (ImGui.Combo("##capmode", ref cap, CaptureNames, CaptureNames.Length))
        { cfg.CaptureWhen = (CaptureMode)cap; cfg.Save(); }
        if (ImGui.IsItemHovered())
            ImGui.SetTooltip("Always = log everything you see, even out of combat (e.g. Shake It Off in town).\nOnly in combat / Only in a duty restrict when the log records.");

        ImGui.SameLine();
        ImGui.TextDisabled("|");
        ImGui.SameLine();

        if (ImGui.Button(_paused ? "Resume" : "Pause"))
        { _paused = !_paused; if (!_paused) _lastEventCount = -1; }
        if (ImGui.IsItemHovered())
            ImGui.SetTooltip("Freeze the table so streaming events don't move the view while you read.\nCapture keeps running in the background.");
        ImGui.SameLine();
        ImGui.Checkbox("Auto-scroll", ref _autoScroll);
        if (ImGui.IsItemHovered())
            ImGui.SetTooltip("Snap back to the newest row as events arrive.");
        ImGui.SameLine();
        if (ImGui.Button("Jump to latest")) _scrollToLatest = true;
        if (_paused)
        {
            ImGui.SameLine();
            ImGui.TextColored(Ui.Gold, "paused");
        }
    }

    private void DrawKindToggles(Configuration cfg)
    {
        ImGui.AlignTextToFramePadding();
        ImGui.TextDisabled("Show:");
        ImGui.SameLine();
        DrawToggle("Casts",   () => cfg.ShowCasts,   v => cfg.ShowCasts = v);   ImGui.SameLine();
        DrawToggle("Status",  () => cfg.ShowStatus,  v => cfg.ShowStatus = v);  ImGui.SameLine();
        DrawToggle("Markers", () => cfg.ShowMarkers, v => cfg.ShowMarkers = v); ImGui.SameLine();
        DrawToggle("Deaths",  () => cfg.ShowDeaths,  v => cfg.ShowDeaths = v);

        ImGui.SameLine();
        ImGui.TextDisabled("|");
        ImGui.SameLine();
        DrawToggle("Enemy", () => cfg.ShowEnemies, v => cfg.ShowEnemies = v); ImGui.SameLine();
        DrawToggle("You",   () => cfg.ShowYou,     v => cfg.ShowYou = v);     ImGui.SameLine();
        DrawToggle("Party", () => cfg.ShowParty,   v => cfg.ShowParty = v);

        ImGui.SameLine();
        ImGui.TextDisabled("|");
        ImGui.SameLine();
        if (ImGui.SmallButton("Reset filters")) ResetFilters(cfg);
        if (ImGui.IsItemHovered())
            ImGui.SetTooltip("Show every event kind again and clear the search / focus / pull filters.");
        ImGui.SameLine();
        ImGui.TextDisabled(FilterSummary(cfg));
    }

    private void DrawAuthorToggles(Configuration cfg)
    {
        ImGui.AlignTextToFramePadding();
        ImGui.TextDisabled("Author:");
        ImGui.SameLine();
        DrawToggle("MapFx",  () => cfg.ShowMapFx,     v => cfg.ShowMapFx = v);     ImGui.SameLine();
        DrawToggle("Adds",   () => cfg.ShowAdds,      v => cfg.ShowAdds = v);      ImGui.SameLine();
        DrawToggle("Ctrl",   () => cfg.ShowControl,   v => cfg.ShowControl = v);
        if (ImGui.IsItemHovered())
            ImGui.SetTooltip("Raw ActorControl opcodes (director commands, knockbacks, etc.).\nVery noisy — only enable when debugging a specific low-level mechanic.");
        ImGui.SameLine();
        DrawToggle("Pos",    () => cfg.ShowPositions, v => cfg.ShowPositions = v);
        ImGui.SameLine();
        DrawToggle("VFX",    () => cfg.LogGameVfx,    v => { cfg.LogGameVfx = v; cfg.ShowVfx = v; });
        if (ImGui.IsItemHovered())
            ImGui.SetTooltip("Experimental: actor-attached effect VFX (the visual 'tells' the game plays on players/bosses).\nVery high volume — capture only runs while this is on. Paste a logged path into a draw's Custom look.");
        ImGui.SameLine();
        ImGui.TextDisabled("|");
        ImGui.SameLine();
        DrawToggle("IDs",     () => cfg.ShowIds,    v => cfg.ShowIds = v);    ImGui.SameLine();
        DrawToggle("Dec",     () => cfg.ShowDecIds, v => cfg.ShowDecIds = v);
        if (ImGui.IsItemHovered())
            ImGui.SetTooltip("Show decimal IDs too (e.g. NpcBaseId for onAdd).");
    }

    private void DrawSearchRow()
    {
        ImGui.AlignTextToFramePadding();
        ImGui.TextDisabled("Find:");
        ImGui.SameLine();
        ImGui.SetNextItemWidth(240f * ImGuiHelpers.GlobalScale);
        ImGui.InputTextWithHint("##search", "name / ID hex+dec…", ref _search, 64);
        ImGui.SameLine();
        ImGui.TextDisabled("in");
        ImGui.SameLine();
        ImGui.SetNextItemWidth(86f * ImGuiHelpers.GlobalScale);
        int scope = (int)_searchScope;
        if (ImGui.Combo("##scope", ref scope, SearchScopeNames, SearchScopeNames.Length))
            _searchScope = (SearchScope)scope;
        if (ImGui.IsItemHovered())
            ImGui.SetTooltip("Limit the text match to one column:\nany = name / source / target / IDs, or pick source / target / ability.");

        ImGui.SameLine();
        ImGui.TextDisabled("|");
        ImGui.SameLine();
        if (ImGui.Button("Wipe log")) { _plugin.Capture.Clear(); _lastEventCount = -1; }
        if (ImGui.IsItemHovered())
            ImGui.SetTooltip("Discard every captured event and reset the pull list.");
        ImGui.SameLine();
        if (ImGui.Button("Export")) ExportLog();
        if (ImGui.IsItemHovered())
            ImGui.SetTooltip("Write a readable text log (relative timings + all fields) and open the folder.");
        ImGui.SameLine();
        if (ImGui.Button("Export JSON")) ExportJson();
        if (ImGui.IsItemHovered())
            ImGui.SetTooltip("Write a structured JSON dump with everything needed to build a module\n" +
                             "(action ids, cast times, positions/headings, targets, statuses, tethers, headmarkers).");
        if (!string.IsNullOrEmpty(_exportStatus))
        {
            ImGui.SameLine();
            ImGui.TextColored(ColParty, _exportStatus);
        }
    }

    private void DrawFocusBanner()
    {
        if (_focusId == 0) return;

        string who = string.IsNullOrEmpty(_focusName) ? $"0x{_focusId:X8}" : _focusName;
        ImGui.AlignTextToFramePadding();
        ImGui.TextColored(Ui.Gold, "Focus:");
        ImGui.SameLine();
        ImGui.TextColored(Ui.Accent, $"{who}  (0x{_focusId:X8})");
        ImGui.SameLine();
        if (ImGui.SmallButton("clear focus")) ClearFocus();
        ImGui.SameLine();
        ImGui.TextDisabled("only events with this actor as source or target are shown");
    }

    private void SetFocus(uint id, string name)
    {
        _focusId = id;
        _focusName = name;
        _lastEventCount = -1;
    }

    private void ClearFocus()
    {
        _focusId = 0;
        _focusName = "";
        _lastEventCount = -1;
    }

    private void ResetFilters(Configuration cfg)
    {
        cfg.ShowCasts = cfg.ShowStatus = cfg.ShowMarkers = cfg.ShowDeaths = true;
        cfg.ShowEnemies = cfg.ShowYou = cfg.ShowParty = true;
        cfg.ShowMapFx = cfg.ShowAdds = cfg.ShowControl = cfg.ShowPositions = true;
        cfg.ShowVfx = true;
        cfg.ShowIds = true;
        cfg.ShowDecIds = false;
        cfg.Save();

        _search = "";
        _searchScope = SearchScope.Any;
        _pullFilter = 0;
        ClearFocus();
    }

    private static string FilterSummary(Configuration cfg)
    {
        var off = new List<string>();
        if (!cfg.ShowCasts)     off.Add("Casts");
        if (!cfg.ShowStatus)    off.Add("Status");
        if (!cfg.ShowMarkers)   off.Add("Markers");
        if (!cfg.ShowDeaths)    off.Add("Deaths");
        if (!cfg.ShowEnemies)   off.Add("Enemy");
        if (!cfg.ShowYou)       off.Add("You");
        if (!cfg.ShowParty)     off.Add("Party");
        if (!cfg.ShowMapFx)     off.Add("MapFx");
        if (!cfg.ShowAdds)      off.Add("Adds");
        if (!cfg.ShowControl)   off.Add("Ctrl");
        if (!cfg.ShowPositions) off.Add("Pos");
        if (!cfg.ShowVfx)       off.Add("VFX");

        if (off.Count == 0) return "all kinds shown";
        const int max = 4;
        return off.Count <= max
            ? $"{off.Count} hidden: {string.Join(", ", off)}"
            : $"{off.Count} hidden: {string.Join(", ", off.GetRange(0, max))}, +{off.Count - max}";
    }

    // Context an author needs at a glance: the arena/zone (territory) id used for
    // zone gating, plus the active boss's name, BNpcBase id and entity id.
    private void DrawZoneBar()
    {
        uint zone = Plugin.ClientState.TerritoryType;
        var  boss = _plugin.Host.FightName;

        uint bossBase = 0, bossEntity = 0;
        if (!string.IsNullOrEmpty(boss) && boss != "(none)")
        {
            foreach (var o in Plugin.ObjectTable)
            {
                if (o is IBattleChara bc &&
                    string.Equals(bc.Name.TextValue, boss, StringComparison.OrdinalIgnoreCase))
                {
                    bossBase   = bc.BaseId;
                    bossEntity = bc.EntityId;
                    break;
                }
            }
        }

        ImGui.TextColored(ColId, $"Zone/Arena ID: {zone}");
        ImGui.SameLine();
        if (ImGui.SmallButton($"copy##zone")) ImGui.SetClipboardText(zone.ToString());

        ImGui.SameLine();
        ImGui.TextDisabled("  |  Boss:");
        ImGui.SameLine();
        ImGui.TextColored(ColEnemy, string.IsNullOrEmpty(boss) ? "(none)" : boss);
        if (bossBase != 0)
        {
            ImGui.SameLine();
            ImGui.TextColored(ColId, $"BaseId {bossBase} (0x{bossBase:X})  Entity 0x{bossEntity:X8}");
            ImGui.SameLine();
            if (ImGui.SmallButton("copy##bossbase")) ImGui.SetClipboardText(bossBase.ToString());
        }
        ImGui.Separator();
    }

    private void DrawPullSidebar()
    {
        float w = 150f * ImGuiHelpers.GlobalScale;
        if (!ImGui.BeginChild("##pulls", new Vector2(w, 0), true))
        {
            ImGui.EndChild();
            return;
        }

        ImGui.TextDisabled("Pulls");
        ImGui.Separator();

        if (ImGui.Selectable("All", _pullFilter == 0))
            _pullFilter = 0;

        var pulls = _plugin.Capture.Pulls;
        for (int i = pulls.Count - 1; i >= 0; i--)
        {
            var p = pulls[i];
            bool selected = _pullFilter == p.Index;
            string label = $"{p.Label}\n{p.Start:HH:mm:ss} · {p.Duration()} · {p.Events}";
            if (ImGui.Selectable($"{label}##pull{p.Index}", selected, ImGuiSelectableFlags.None,
                    new Vector2(0, ImGui.GetTextLineHeight() * 2f)))
                _pullFilter = p.Index;
        }

        ImGui.EndChild();
    }

    // Filtered view, newest-first. Rebuilt whenever the inputs that shape it move.
    private readonly List<LogEvent> _filtered = new();
    private int         _lastEventCount = -1;
    private int         _lastPullFilter = -1;
    private string      _lastSearch     = "";
    private SearchScope _lastSearchScope;
    private uint        _lastFocusId;

    private void RebuildFiltered()
    {
        // While frozen the list stays exactly as it was when paused.
        if (_paused) return;

        var events = _plugin.Capture.Events;
        if (events.Count == _lastEventCount
            && _pullFilter  == _lastPullFilter
            && _search      == _lastSearch
            && _searchScope == _lastSearchScope
            && _focusId     == _lastFocusId)
            return;

        _lastEventCount  = events.Count;
        _lastPullFilter  = _pullFilter;
        _lastSearch      = _search;
        _lastSearchScope = _searchScope;
        _lastFocusId     = _focusId;

        _filtered.Clear();
        for (int i = events.Count - 1; i >= 0; i--)
        {
            var e = events[i];
            if (Passes(e)) _filtered.Add(e);
        }
    }

    private void DrawTable()
    {
        RebuildFiltered();

        if (!_paused && _autoScroll && _filtered.Count != _prevFilteredCount)
            _scrollToLatest = true;
        _prevFilteredCount = _filtered.Count;

        if (!ImGui.BeginTable("##log", 5,
                ImGuiTableFlags.RowBg | ImGuiTableFlags.ScrollY | ImGuiTableFlags.BordersInnerH))
            return;

        ImGui.TableSetupScrollFreeze(0, 1);
        ImGui.TableSetupColumn("Time",   ImGuiTableColumnFlags.WidthFixed, 58f * ImGuiHelpers.GlobalScale);
        ImGui.TableSetupColumn("Source", ImGuiTableColumnFlags.WidthFixed, 140f * ImGuiHelpers.GlobalScale);
        ImGui.TableSetupColumn("Event",  ImGuiTableColumnFlags.WidthFixed, 70f * ImGuiHelpers.GlobalScale);
        ImGui.TableSetupColumn("Target", ImGuiTableColumnFlags.WidthFixed, 140f * ImGuiHelpers.GlobalScale);
        ImGui.TableSetupColumn("Detail", ImGuiTableColumnFlags.WidthStretch);
        ImGui.TableHeadersRow();

        // Newest-first means the latest row sits at the top.
        if (_scrollToLatest) { ImGui.SetScrollY(0f); _scrollToLatest = false; }

        // Let the clipper own the spacing math so the scrollbar stays in sync
        // across a whole fight's worth of rows (50 000+).
        float rowH = ImGui.GetTextLineHeightWithSpacing();
        var clipper = new ImGuiListClipper();
        clipper.Begin(_filtered.Count, rowH);
        while (clipper.Step())
            for (int i = clipper.DisplayStart; i < clipper.DisplayEnd; i++)
                DrawRow(_filtered[i]);
        clipper.End();

        ImGui.EndTable();
    }

    private bool Passes(LogEvent e)
    {
        var cfg = _plugin.Configuration;

        if (_pullFilter != 0 && e.Pull != _pullFilter) return false;

        if (_focusId != 0 && !TouchesFocus(e)) return false;

        bool kindOk = e.Kind switch
        {
            LogKind.CastStart or LogKind.CastFinish or LogKind.Ability => cfg.ShowCasts,
            LogKind.StatusGain or LogKind.StatusLose                   => cfg.ShowStatus,
            LogKind.Death                                              => cfg.ShowDeaths,
            LogKind.Headmarker or LogKind.Tether                       => cfg.ShowMarkers,
            // Module-authoring feeds, each gated by its own toggle.
            LogKind.MapEffect                                          => cfg.ShowMapFx,
            LogKind.Added                                              => cfg.ShowAdds,
            LogKind.ActorControl                                       => cfg.ShowControl,
            LogKind.AbilityExtra                                       => cfg.ShowPositions,
            LogKind.Vfx                                                => cfg.ShowVfx,
            // Diagnostic notes (JS console.log) are always shown; they're rare and
            // explicitly opt-in from the script side.
            LogKind.Note                                               => true,
            _                                                          => false,
        };
        if (!kindOk) return false;

        // Environmental feeds (MapEffect / generic ActorControl / notes) have no
        // real actor owner, so the Enemy/You/Party filter must not hide them.
        bool isEnv = e.Kind is LogKind.MapEffect or LogKind.ActorControl or LogKind.Note;
        if (!isEnv)
        {
            // Markers and statuses are about who they land ON, so classify them by
            // the target's kind (recorded at capture time). Everything else is
            // classified by its source actor. This is what makes "Enemy only"
            // actually hide player/party-targeted statuses and markers.
            ActorKind who = (e.IsStatus || e.Kind is LogKind.Headmarker or LogKind.Tether)
                ? e.TargetKind
                : e.SourceKind;
            bool srcOk = who switch
            {
                ActorKind.Enemy => cfg.ShowEnemies,
                ActorKind.You   => cfg.ShowYou,
                ActorKind.Party => cfg.ShowParty,
                // Unknown owner: fall back to a live self/party lookup by target id.
                _               => TargetKindOk(e),
            };
            if (!srcOk) return false;
        }

        if (!string.IsNullOrEmpty(_search))
        {
            var s = _search;
            bool hit = _searchScope switch
            {
                SearchScope.Source  => e.SourceName.Contains(s, StringComparison.OrdinalIgnoreCase),
                SearchScope.Target  => e.TargetName.Contains(s, StringComparison.OrdinalIgnoreCase),
                SearchScope.Ability => e.Name.Contains(s, StringComparison.OrdinalIgnoreCase)
                                       || (e.DataId != 0 && IdMatches(e.DataId, s)),
                _ => e.Name.Contains(s, StringComparison.OrdinalIgnoreCase)
                     || e.SourceName.Contains(s, StringComparison.OrdinalIgnoreCase)
                     || e.TargetName.Contains(s, StringComparison.OrdinalIgnoreCase)
                     || (e.DataId   != 0 && IdMatches(e.DataId, s))
                     || (e.Category != 0 && IdMatches(e.Category, s)),
            };
            if (!hit) return false;
        }

        return true;
    }

    private bool TouchesFocus(LogEvent e)
    {
        if (e.SourceId == _focusId || e.TargetId == _focusId) return true;
        var ids = e.AbilityTargetIds;
        if (ids != null)
            for (int i = 0; i < ids.Length; i++)
                if (ids[i] == _focusId) return true;
        return false;
    }

    private static bool IdMatches(uint id, string search)
    {
        var t = search.Trim();
        if (t.StartsWith("0x", StringComparison.OrdinalIgnoreCase)) t = t[2..];
        if (t.Length == 0) return false;
        if (id.ToString("X").Contains(t, StringComparison.OrdinalIgnoreCase)) return true;
        if (id.ToString("X4").Contains(t, StringComparison.OrdinalIgnoreCase)) return true;
        return id.ToString().Contains(t, StringComparison.Ordinal);
    }

    private bool TargetKindOk(LogEvent e)
    {
        var cfg = _plugin.Configuration;
        if (e.TargetId == Plugin.PlayerState.EntityId) return cfg.ShowYou;
        foreach (var m in Plugin.PartyList)
            if (m?.EntityId == e.TargetId) return cfg.ShowParty;
        return cfg.ShowEnemies;
    }

    private void DrawRow(LogEvent e)
    {
        ImGui.TableNextRow();
        ImGui.TableNextColumn();

        if (ImGui.Selectable($"{e.Time:HH:mm:ss}##r{e.Seq}", false, ImGuiSelectableFlags.SpanAllColumns))
        {
        }
        if (ImGui.BeginPopupContextItem($"ctx{e.Seq}"))
        {
            ImGui.TextDisabled(string.IsNullOrEmpty(e.Name) ? e.Kind.ToString() : e.Name);
            if (e.DataId != 0)
            {
                ImGui.SameLine();
                ImGui.TextColored(ColId, $"[{e.DataId:X4} · {e.DataId}]");
            }
            ImGui.Separator();

            if (ImGui.MenuItem("Create quick draw from this"))
                _plugin.OpenQuickDrawFor(e);
            ImGui.Separator();

            if (e.SourceId != 0 && e.SourceId != _focusId &&
                ImGui.MenuItem(string.IsNullOrEmpty(e.SourceName)
                    ? $"Focus this source  (0x{e.SourceId:X8})"
                    : $"Focus this source  ({e.SourceName})"))
                SetFocus(e.SourceId, e.SourceName);
            if (e.TargetId != 0 && e.TargetId != _focusId &&
                ImGui.MenuItem(string.IsNullOrEmpty(e.TargetName)
                    ? $"Focus this target  (0x{e.TargetId:X8})"
                    : $"Focus this target  ({e.TargetName})"))
                SetFocus(e.TargetId, e.TargetName);
            if (_focusId != 0 && ImGui.MenuItem("Clear focus"))
                ClearFocus();
            ImGui.Separator();

            if (e.DataId != 0)
            {
                if (ImGui.MenuItem($"Copy ID hex  ({e.DataId:X})"))
                    ImGui.SetClipboardText(e.DataId.ToString("X"));
                if (ImGui.MenuItem($"Copy ID dec  ({e.DataId})"))
                    ImGui.SetClipboardText(e.DataId.ToString());
            }
            if (e.Kind == LogKind.MapEffect)
            {
                if (ImGui.MenuItem($"Copy flags  ({e.Category:X8})"))
                    ImGui.SetClipboardText(e.Category.ToString("X8"));
                if (ImGui.MenuItem($"Copy location  ({e.Param1:X2})"))
                    ImGui.SetClipboardText(e.Param1.ToString("X2"));
            }
            if (!string.IsNullOrEmpty(e.SourceName) && ImGui.MenuItem($"Copy source name  ({e.SourceName})"))
                ImGui.SetClipboardText(e.SourceName);
            if (!string.IsNullOrEmpty(e.TargetName) && ImGui.MenuItem($"Copy target name  ({e.TargetName})"))
                ImGui.SetClipboardText(e.TargetName);
            if (e.SourceId != 0 && ImGui.MenuItem($"Copy source entity  (0x{e.SourceId:X8})"))
                ImGui.SetClipboardText($"{e.SourceId:X8}");
            if (e.TargetId != 0 && ImGui.MenuItem($"Copy target entity  (0x{e.TargetId:X8})"))
                ImGui.SetClipboardText($"{e.TargetId:X8}");

            ImGui.EndPopup();
        }

        ImGui.TableNextColumn();
        DrawActorCell(e.SourceName, e.SourceId, SourceColor(e));

        ImGui.TableNextColumn();
        var (label, col) = e.Kind switch
        {
            LogKind.CastStart    => ("startcast", ColCast),
            LogKind.CastFinish   => ("endcast",   ColCast),
            LogKind.Ability      => ("use",    ColUse),
            LogKind.StatusGain   => ("gain",   ColGain),
            LogKind.StatusLose   => ("lose",   ColLose),
            LogKind.Death        => ("death",  ColDeath),
            LogKind.Headmarker   => ("marker", ColMarker),
            LogKind.Tether       => ("tether", ColMarker),
            LogKind.MapEffect    => ("mapfx",  ColMap),
            LogKind.Added        => ("add",    ColEnemy),
            LogKind.ActorControl => ("ctrl",   ColCtrl),
            LogKind.AbilityExtra => ("pos",    ColCtrl),
            LogKind.Vfx          => ("vfx",    ColMap),
            LogKind.Note         => ("note",   ColNote),
            _                    => ("?",      ColDim),
        };
        ImGui.TextColored(col, label);

        ImGui.TableNextColumn();
        DrawActorCell(e.TargetName, e.TargetId, TargetColor(e));

        ImGui.TableNextColumn();
        DrawIcon(e.IconId, ImGui.GetTextLineHeight());
        ImGui.SameLine();
        switch (e.Kind)
        {
            case LogKind.CastStart:
            case LogKind.CastFinish:
            case LogKind.Ability:
                ImGui.Text(e.Name);
                DrawId(e);
                if (e.Value > 0)
                {
                    ImGui.SameLine();
                    ImGui.TextDisabled($"({e.Value:0.0}s)");
                }
                if (!string.IsNullOrEmpty(e.TargetName))
                {
                    ImGui.SameLine();
                    ImGui.TextDisabled($"→ {e.TargetName}");
                }
                break;
            case LogKind.StatusGain:
            case LogKind.StatusLose:
                ImGui.Text(e.Name);
                DrawId(e);
                ImGui.SameLine();
                ImGui.TextDisabled($"on {e.TargetName}");
                if (e.Kind == LogKind.StatusGain && e.Value > 0)
                {
                    ImGui.SameLine();
                    ImGui.TextDisabled($"({e.Value:0.0}s)");
                }
                break;
            case LogKind.Headmarker:
                ImGui.Text(e.Name);
                DrawId(e);
                if (!string.IsNullOrEmpty(e.TargetName))
                {
                    ImGui.SameLine();
                    ImGui.TextDisabled($"on {e.TargetName}");
                }
                break;
            case LogKind.Tether:
                ImGui.Text(e.Name);
                DrawId(e);
                ImGui.SameLine();
                ImGui.TextDisabled($"{e.SourceName} → {e.TargetName}");
                break;
            case LogKind.Death:
                ImGui.TextColored(ColDeath, $"{e.SourceName} died");
                break;
            case LogKind.MapEffect:
                ImGui.TextColored(ColMap, "MapEffect");
                ImGui.SameLine();
                ImGui.TextColored(ColId, $"loc {e.Param1:X2}");
                ImGui.SameLine();
                ImGui.TextDisabled($"({e.Param1})");
                ImGui.SameLine();
                ImGui.TextColored(ColId, $"flags {e.Category:X8}");
                break;
            case LogKind.Added:
                ImGui.Text(e.Name);
                ImGui.SameLine();
                ImGui.TextColored(ColId, $"[BaseId {e.DataId:X} · {e.DataId}]");
                if (e.X != 0 || e.Y != 0)
                {
                    ImGui.SameLine();
                    ImGui.TextDisabled($"@ ({e.X:0.0}, {e.Y:0.0})");
                }
                break;
            case LogKind.ActorControl:
                ImGui.TextColored(ColCtrl, $"cat {e.Category:X4}");
                ImGui.SameLine();
                ImGui.TextDisabled($"({e.Category})  p1 {e.Param1:X}  p2 {e.Param2:X}  p3 {e.Param3:X}  p4 {e.Param4:X}");
                if (!string.IsNullOrEmpty(e.SourceName))
                {
                    ImGui.SameLine();
                    ImGui.TextDisabled($"← {e.SourceName}");
                }
                break;
            case LogKind.Note:
                ImGui.TextColored(ColNote, e.Name);
                break;
            case LogKind.AbilityExtra:
                ImGui.Text(string.IsNullOrEmpty(e.Name) ? "effect" : e.Name);
                DrawId(e);
                ImGui.SameLine();
                ImGui.TextDisabled($"@ ({e.X:0.0}, {e.Y:0.0})");
                break;
            case LogKind.Vfx:
                ImGui.TextColored(ColMap, e.Name);
                if (!string.IsNullOrEmpty(e.TargetName))
                {
                    ImGui.SameLine();
                    ImGui.TextDisabled($"on {e.TargetName}");
                }
                else if (!string.IsNullOrEmpty(e.SourceName))
                {
                    ImGui.SameLine();
                    ImGui.TextDisabled($"from {e.SourceName}");
                }
                break;
        }
    }

    private void DrawId(LogEvent e)
    {
        var cfg = _plugin.Configuration;
        if (!cfg.ShowIds || e.DataId == 0) return;
        ImGui.SameLine();
        // Triggers match on hex; some APIs (e.g. NpcBaseId for onAdd) expect
        // decimal, so show both when requested.
        ImGui.TextColored(ColId, cfg.ShowDecIds ? $"[{e.DataId:X4} · {e.DataId}]" : $"[{e.DataId:X4}]");
    }

    private Dictionary<int, DateTime> PullStarts()
    {
        var d = new Dictionary<int, DateTime>();
        foreach (var p in _plugin.Capture.Pulls) d[p.Index] = p.Start;
        return d;
    }

    // Exports every captured event, ignoring the on-screen filters.
    private void ExportLog()
    {
        try
        {
            var events = _plugin.Capture.Events;
            var starts = PullStarts();
            var sb = new StringBuilder(events.Count * 72);
            sb.AppendLine($"# YapYap fight log  ({DateTime.Now:yyyy-MM-dd HH:mm:ss})");
            sb.AppendLine($"# Zone/Arena ID: {Plugin.ClientState.TerritoryType}   Active fight: {_plugin.Host.FightName}");
            sb.AppendLine($"# {events.Count} events   ({_plugin.Capture.Pulls.Count} pulls)");
            sb.AppendLine("# columns: [pull +relSec] [clock] kind  source -> target : detail");
            sb.AppendLine();

            for (int i = 0; i < events.Count; i++)
            {
                var e = events[i];
                if (_pullFilter != 0 && e.Pull != _pullFilter) continue;
                double rel = starts.TryGetValue(e.Pull, out var s) ? (e.Time - s).TotalSeconds : 0;
                sb.Append("[p").Append(e.Pull).Append(" +").Append(rel.ToString("00.000")).Append("] ");
                sb.Append('[').Append(e.Time.ToString("HH:mm:ss.fff")).Append("] ");
                sb.AppendLine(FormatExportLine(e));
            }

            var path = WriteExport("txt", sb.ToString());
            _exportStatus = $"saved → {path}";
            RevealFile(path);
        }
        catch (Exception ex)
        {
            _exportStatus = $"export failed: {ex.Message}";
        }
    }

    // Structured dump with everything needed to author a fight module.
    private void ExportJson()
    {
        try
        {
            var cap    = _plugin.Capture;
            var events = cap.Events;
            var starts = PullStarts();

            var pulls = cap.Pulls
                .Where(p => _pullFilter == 0 || p.Index == _pullFilter)
                .Select(p => new
                {
                    index       = p.Index,
                    label       = p.Label,
                    territory   = p.Territory,
                    mapId       = p.MapId,
                    start       = p.Start.ToString("o"),
                    durationSec = Math.Round(((p.End == DateTime.MinValue ? DateTime.Now : p.End) - p.Start).TotalSeconds, 2),
                    events      = p.Events,
                })
                .ToList();

            var evs = new List<object>(events.Count);
            foreach (var e in events)
            {
                if (_pullFilter != 0 && e.Pull != _pullFilter) continue;
                double rel = starts.TryGetValue(e.Pull, out var s) ? (e.Time - s).TotalSeconds : 0;
                evs.Add(new
                {
                    t      = Math.Round(rel, 3),
                    pull   = e.Pull,
                    seq    = e.Seq,
                    kind   = e.Kind.ToString(),
                    source = new { name = e.SourceName, id = $"0x{e.SourceId:X8}", kind = e.SourceKind.ToString() },
                    target = new { name = e.TargetName, id = $"0x{e.TargetId:X8}", kind = e.TargetKind.ToString() },
                    action = new { name = e.Name, id = e.DataId, idHex = $"0x{e.DataId:X}" },
                    iconId     = e.IconId,
                    castSec    = Math.Round(e.Value, 3),
                    count      = e.Count,
                    pos        = new { x = Math.Round(e.X, 3), z = Math.Round(e.Y, 3) },
                    headingRad = Math.Round(e.Heading, 4),
                    headingDeg = Math.Round(e.Heading * 180.0 / Math.PI, 1),
                    category   = e.Category,
                    @params    = new[] { e.Param1, e.Param2, e.Param3, e.Param4 },
                    targets    = e.AbilityTargetIds.Select(t => $"0x{t:X8}").ToArray(),
                });
            }

            var root = new
            {
                exported   = DateTime.Now.ToString("o"),
                zone       = Plugin.ClientState.TerritoryType,
                fight      = _plugin.Host.FightName,
                pullFilter = _pullFilter,
                legend = new
                {
                    t          = "seconds since the start of that event's pull",
                    pos        = "world coordinates: x = east/west, z = north/south",
                    heading    = "facing in radians (headingRad) and degrees (headingDeg)",
                    action     = "id = Lumina Action sheet row (cast/ability)",
                    castSec    = "cast duration for CastStart events",
                    iconId     = "headmarker / status icon id",
                    targets    = "entity ids hit by an Ability",
                    kinds      = Enum.GetNames(typeof(LogKind)),
                },
                pulls,
                events = evs,
            };

            var json = JsonSerializer.Serialize(root, new JsonSerializerOptions { WriteIndented = true });
            var path = WriteExport("json", json);
            _exportStatus = $"saved → {path}";
            RevealFile(path);
        }
        catch (Exception ex)
        {
            _exportStatus = $"json export failed: {ex.Message}";
        }
    }

    private string WriteExport(string ext, string contents)
    {
        var dir = Plugin.PluginInterface.GetPluginConfigDirectory();
        Directory.CreateDirectory(dir);
        string pull = _pullFilter != 0 ? $"-pull{_pullFilter}" : "";
        var path = Path.Combine(dir, $"yapyap-log-{DateTime.Now:yyyyMMdd-HHmmss}{pull}.{ext}");
        File.WriteAllText(path, contents);
        return path;
    }

    private static void RevealFile(string path)
    {
        try { Process.Start(new ProcessStartInfo("explorer.exe", $"/select,\"{path}\"") { UseShellExecute = true }); }
        catch { /* opening the folder is best-effort */ }
    }

    private static string FormatExportLine(LogEvent e)
    {
        string id  = e.DataId != 0 ? $"[{e.DataId:X} / {e.DataId}]" : "";
        string src = e.SourceId != 0 ? $"{e.SourceName}(0x{e.SourceId:X8})" : e.SourceName;
        string tgt = e.TargetId != 0 ? $"{e.TargetName}(0x{e.TargetId:X8})" : e.TargetName;

        return e.Kind switch
        {
            LogKind.CastStart    => $"startcast {src} -> {tgt} : {e.Name} {id} ({e.Value:0.0}s)",
            LogKind.CastFinish   => $"endcast   {src} -> {tgt} : {e.Name} {id}",
            LogKind.Ability      => $"use     {src} -> {tgt} : {e.Name} {id}",
            LogKind.StatusGain   => $"gain    {e.Name} {id} on {tgt} ({e.Value:0.0}s)",
            LogKind.StatusLose   => $"lose    {e.Name} {id} on {tgt}",
            LogKind.Death        => $"death   {src}",
            LogKind.Headmarker   => $"marker  {e.Name} {id} on {tgt}",
            LogKind.Tether       => $"tether  {e.Name} {id} {src} -> {tgt}",
            LogKind.MapEffect    => $"mapfx   location {e.Param1:X2} ({e.Param1})  flags {e.Category:X8}",
            LogKind.Added        => $"add     {e.Name} [BaseId {e.DataId:X} / {e.DataId}] @ ({e.X:0.0}, {e.Y:0.0})",
            LogKind.ActorControl => $"ctrl    cat {e.Category:X4} ({e.Category})  p1 {e.Param1:X} p2 {e.Param2:X} p3 {e.Param3:X} p4 {e.Param4:X}  src {src}",
            LogKind.AbilityExtra => $"pos     {src} : {e.Name} {id} @ ({e.X:0.0}, {e.Y:0.0})",
            LogKind.Vfx          => $"vfx     {e.Name}  on {tgt}  from {src}",
            LogKind.Note         => $"note    {e.Name}",
            _                    => $"?       {e.Name} {id}",
        };
    }

    private static Vector4 SourceColor(LogEvent e) => e.SourceKind switch
    {
        ActorKind.Enemy => ColEnemy,
        ActorKind.You   => ColYou,
        ActorKind.Party => ColParty,
        _               => ColDim,
    };

    private static Vector4 TargetColor(LogEvent e) => e.TargetKind switch
    {
        ActorKind.Enemy => ColEnemy,
        ActorKind.You   => ColYou,
        ActorKind.Party => ColParty,
        _               => ColDim,
    };

    // Name (colored by actor kind) plus the entity id when IDs are on. The id is
    // a hover-to-copy hex; a gold tint flags the currently focused actor.
    private void DrawActorCell(string name, uint id, Vector4 col)
    {
        bool focused = id != 0 && id == _focusId;
        if (!string.IsNullOrEmpty(name))
            ImGui.TextColored(focused ? Ui.Gold : col, name);

        if (!_plugin.Configuration.ShowIds || id == 0) return;

        if (!string.IsNullOrEmpty(name)) ImGui.SameLine();
        ImGui.TextColored(focused ? Ui.Gold : ColId, $"0x{id:X8}");
        if (ImGui.IsItemHovered())
            ImGui.SetTooltip("click to copy entity id");
        if (ImGui.IsItemClicked()) ImGui.SetClipboardText($"{id:X8}");
    }

    private void DrawToggle(string label, Func<bool> get, Action<bool> set)
    {
        bool v = get();
        if (ImGui.Checkbox(label, ref v)) { set(v); _plugin.Configuration.Save(); _lastEventCount = -1; }
    }

    private void DrawIcon(uint iconId, float size)
    {
        if (iconId == 0) { ImGui.Dummy(new Vector2(size, size)); return; }
        if (!_iconCache.TryGetValue(iconId, out var tex))
        {
            if (_iconCache.Count > 256) _iconCache.Clear();
            tex = Plugin.TextureProvider.GetFromGameIcon(new GameIconLookup(iconId));
            _iconCache[iconId] = tex;
        }
        var wrap = tex?.GetWrapOrDefault();
        if (wrap != null) ImGui.Image(wrap.Handle, new Vector2(size, size));
        else ImGui.Dummy(new Vector2(size, size));
    }
}
