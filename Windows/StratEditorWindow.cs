using System;
using System.Collections.Generic;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Windowing;
using YapYapDraw.Logging;
using YapYapDraw.QuickDraws;
using YapYapDraw.Strats;
using LuminaAction = Lumina.Excel.Sheets.Action;

namespace YapYapDraw.Windows;

public sealed class StratEditorWindow : Window, IDisposable
{
    private readonly Plugin _plugin;
    private readonly MapCanvas _canvas;
    private StratPack? _pack;

    private int  _slideIdx  = -1;
    private int  _branchIdx = -1;
    private StratRole _placing = StratRole.MT;
    private bool _snap;

    private static readonly string[] RoleNames = { "MT", "OT", "M1", "M2", "R1", "R2", "H1", "H2" };

    private static readonly string[]       OnNames = { "Cast start", "Cast end", "Status gain", "Status lose", "Headmarker", "Tether", "Death", "Any" };
    private static readonly TriggerMatch[]  OnVals = { TriggerMatch.Cast, TriggerMatch.CastEnd, TriggerMatch.StatusGain, TriggerMatch.StatusLose, TriggerMatch.Headmarker, TriggerMatch.Tether, TriggerMatch.Death, TriggerMatch.Any };

    private static readonly string[]  CondKindNames = { "My debuff/status", "My role", "Boss position", "Tether on me" };
    private static readonly string[]  RoleCatNames  = { "Tank", "Healer", "DPS (any)", "Melee", "Ranged" };
    private static readonly string[]    CompassNames = { "N", "NE", "E", "SE", "S", "SW", "W", "NW" };
    private static readonly string[]      ShapeNames = { "Circle", "Tower", "Donut", "Rectangle", "Fan" };
    private static readonly QuickShape[]  ShapeVals  = { QuickShape.Circle, QuickShape.Tower, QuickShape.Donut, QuickShape.Rectangle, QuickShape.Fan };

    public StratEditorWindow(Plugin plugin)
        : base("Strat Builder###YapYapDrawStrat")
    {
        _plugin = plugin;
        _canvas = new MapCanvas(plugin);
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(820, 480),
            MaximumSize = new Vector2(2000, 1600),
        };
        Size          = new Vector2(1040, 720);
        SizeCondition = ImGuiCond.FirstUseEver;
    }

    public void Dispose() { }

    public override void PreDraw()  => Ui.PushTheme();
    public override void PostDraw() => Ui.PopTheme();

    public void Open(StratPack pack)
    {
        _pack      = pack;
        _slideIdx  = pack.Slides.Count > 0 ? 0 : -1;
        _branchIdx = pack.Slides.Count > 0 && pack.Slides[0].Branches.Count > 0 ? 0 : -1;
        IsOpen     = true;
        BringToFront();
        _canvas.RecenterOnPlayer();
    }

    public override void Draw()
    {
        if (_pack == null)
        {
            ImGui.TextColored(Ui.Dimmed, "Open a strat from the Strats tab.");
            return;
        }

        var cfg   = _plugin.Configuration;
        float scale = ImGuiHelpers.GlobalScale;

        ImGui.SetNextItemWidth(240f * scale);
        string name = _pack.Name;
        if (ImGui.InputText("##sname", ref name, 64)) { _pack.Name = name; cfg.Save(); }

        ImGui.SameLine();
        ImGui.AlignTextToFramePadding();
        ImGui.TextColored(Ui.Dimmed, $"zone {_pack.Territory}");
        ImGui.SameLine();
        if (ImGui.SmallButton("Use current zone"))
        {
            _pack.Territory = Plugin.ClientState.TerritoryType;
            cfg.Save();
        }
        ImGui.SameLine();
        if (ImGui.SmallButton("Arena")) ImGui.OpenPopup("##arenacfg");
        DrawArenaSettings(cfg);

        ImGui.Separator();

        var slide  = (_slideIdx  >= 0 && _slideIdx  < _pack.Slides.Count) ? _pack.Slides[_slideIdx] : null;
        var branch = (slide != null && _branchIdx >= 0 && _branchIdx < slide.Branches.Count) ? slide.Branches[_branchIdx] : null;

        ImGui.BeginChild("##stratleft", new Vector2(360f * scale, 0), false);

        ImGui.BeginChild("##castfeed", new Vector2(0, 190f * scale), true);
        DrawCastFeed(cfg);
        ImGui.EndChild();

        ImGui.BeginChild("##stepsbox", new Vector2(0, 150f * scale), true);
        DrawSteps(cfg);
        ImGui.EndChild();

        ImGui.BeginChild("##branchbox", new Vector2(0, 0), true);
        if (slide != null) DrawBranches(cfg, slide);
        if (slide != null && ImGui.CollapsingHeader("Edit trigger manually"))
            DrawTrigger(cfg, slide);
        ImGui.EndChild();

        ImGui.EndChild();

        ImGui.SameLine();

        ImGui.BeginChild("##stratright", new Vector2(0, 0), false);
        if (branch != null) DrawArenaPane(cfg, slide!, branch, scale);
        else ImGui.TextColored(Ui.Dimmed, "Add a step and a branch to start placing spots.");
        ImGui.EndChild();
    }

    private void DrawArenaSettings(Configuration cfg)
    {
        if (!ImGui.BeginPopup("##arenacfg")) return;
        float scale = ImGuiHelpers.GlobalScale;

        ImGui.TextColored(Ui.Gold, "Arena shape (clean ring over the map)");
        int shape = _pack!.ArenaShape;
        ImGui.SetNextItemWidth(140f * scale);
        if (ImGui.Combo("shape##arena", ref shape, new[] { "Circle", "Square" }, 2)) { _pack.ArenaShape = (byte)shape; cfg.Save(); }

        float rad = _pack.ArenaRadius;
        ImGui.SetNextItemWidth(140f * scale);
        if (ImGui.DragFloat("radius (y)##arena", ref rad, 0.25f, 2f, 60f)) { _pack.ArenaRadius = rad; cfg.Save(); }

        var center = new Vector2(_pack.ArenaCenterX, _pack.ArenaCenterZ);
        ImGui.SetNextItemWidth(160f * scale);
        if (ImGui.InputFloat2("center X/Z##arena", ref center)) { _pack.ArenaCenterX = center.X; _pack.ArenaCenterZ = center.Y; cfg.Save(); }

        if (ImGui.Button("Center = my target"))
        {
            var tgt = Plugin.ObjectTable.LocalPlayer?.TargetObject;
            if (tgt != null) { _pack.ArenaCenterX = tgt.Position.X; _pack.ArenaCenterZ = tgt.Position.Z; cfg.Save(); }
        }
        ImGui.SameLine();
        if (ImGui.Button("Center = me"))
        {
            var me = Plugin.ObjectTable.LocalPlayer;
            if (me != null) { _pack.ArenaCenterX = me.Position.X; _pack.ArenaCenterZ = me.Position.Z; cfg.Save(); }
        }
        ImGui.EndPopup();
    }

    private void DrawCastFeed(Configuration cfg)
    {
        float scale = ImGuiHelpers.GlobalScale;
        ImGui.TextColored(Ui.Gold, "Boss casts (live) — click to add a step");

        var seenActive = new HashSet<uint>();
        bool anyActive = false;
        foreach (var o in Plugin.ObjectTable)
        {
            if (o.ObjectKind != ObjectKind.BattleNpc) continue;
            if (o is not IBattleChara bc || !bc.IsCasting) continue;
            if (bc.CastActionId == 0 || !seenActive.Add(bc.CastActionId)) continue;
            anyActive = true;

            float frac = bc.TotalCastTime > 0 ? bc.CurrentCastTime / bc.TotalCastTime : 0f;
            string cn  = ActionName(bc.CastActionId);

            ImGui.PushID($"ac{bc.CastActionId}");
            ImGui.ProgressBar(frac, new Vector2(130f * scale, ImGui.GetFrameHeight()), cn);
            ImGui.SameLine();
            if (ImGui.SmallButton("+")) CreateStepFromCast(cfg, bc.CastActionId, cn, false);
            if (ImGui.IsItemHovered()) ImGui.SetTooltip("Add a step bound to this cast");
            ImGui.SameLine();
            if (ImGui.SmallButton("N/S")) CreateStepFromCast(cfg, bc.CastActionId, cn, true);
            if (ImGui.IsItemHovered()) ImGui.SetTooltip("Add a step split by boss North/South");
            ImGui.PopID();
        }
        if (!anyActive)
            ImGui.TextColored(Ui.Dimmed, "No boss casting now — recent below.");

        var recent = RecentEnemyCasts();
        if (recent.Count > 0)
        {
            ImGui.Spacing();
            ImGui.TextDisabled("Recent:");
            foreach (var (id, nm) in recent)
            {
                ImGui.PushID($"rc{id}");
                if (ImGui.SmallButton($"{nm}##r")) CreateStepFromCast(cfg, id, nm, false);
                if (ImGui.IsItemHovered()) ImGui.SetTooltip($"action #{id}\nclick: add step · shift-click: add N/S split");
                if (ImGui.IsItemClicked() && ImGui.GetIO().KeyShift) CreateStepFromCast(cfg, id, nm, true);
                ImGui.SameLine();
                ImGui.PopID();
            }
            ImGui.NewLine();
        }

        var fires = _plugin.Engine.RecentFires;
        if (fires.Count > 0)
        {
            ImGui.Spacing();
            ImGui.TextDisabled("YapDraw firing:");
            int shown = 0;
            for (int i = fires.Count - 1; i >= 0 && shown < 4; i--, shown++)
                ImGui.TextColored(Ui.Green, $"  {fires[i].Draw}  ({fires[i].Trigger})");
        }
    }

    private void CreateStepFromCast(Configuration cfg, uint actionId, string castName, bool nsSplit)
    {
        var s = new StratSlide
        {
            Name      = string.IsNullOrEmpty(castName) ? $"Cast #{actionId}" : castName,
            On        = TriggerMatch.Cast,
            MatchById = actionId != 0,
            DataId    = actionId,
            Pattern   = castName,
        };

        if (nsSplit)
        {
            var tgt   = Plugin.ObjectTable.LocalPlayer?.TargetObject;
            uint bid  = tgt?.EntityId ?? 0;
            string bn = tgt?.Name.TextValue ?? "";
            var north = new StratBranch { Name = "North" };
            north.Conditions.Add(new StratCondition { Kind = CondKind.BossSide, BossSide = Compass.N, BossId = bid, BossName = bn });
            var south = new StratBranch { Name = "South" };
            south.Conditions.Add(new StratCondition { Kind = CondKind.BossSide, BossSide = Compass.S, BossId = bid, BossName = bn });
            s.Branches.Add(north);
            s.Branches.Add(south);
        }
        else
        {
            s.Branches.Add(new StratBranch { Name = "Default" });
        }

        _pack!.Slides.Add(s);
        _slideIdx  = _pack.Slides.Count - 1;
        _branchIdx = 0;
        cfg.Save();
    }

    private List<(uint id, string name)> RecentEnemyCasts()
    {
        var list = new List<(uint, string)>();
        var seen = new HashSet<uint>();
        var ev = _plugin.Capture.Events;
        for (int i = ev.Count - 1; i >= 0 && list.Count < 10; i--)
        {
            var e = ev[i];
            if (e.Kind != LogKind.CastStart || e.SourceKind != ActorKind.Enemy) continue;
            if (e.DataId == 0 || !seen.Add(e.DataId)) continue;
            list.Add((e.DataId, string.IsNullOrEmpty(e.Name) ? $"#{e.DataId}" : e.Name));
        }
        return list;
    }

    private static string ActionName(uint id)
    {
        if (id == 0) return "";
        var a = Plugin.Actions.GetRowOrDefault(id);
        var n = a?.Name.ExtractText();
        return string.IsNullOrEmpty(n) ? $"#{id}" : n;
    }

    private void DrawSteps(Configuration cfg)
    {
        ImGui.TextColored(Ui.Gold, "Steps");
        ImGui.SameLine();
        if (ImGui.SmallButton("+ Step"))
        {
            var s = new StratSlide { Name = $"Step {_pack!.Slides.Count + 1}" };
            s.Branches.Add(new StratBranch { Name = "Default" });
            _pack.Slides.Add(s);
            _slideIdx  = _pack.Slides.Count - 1;
            _branchIdx = 0;
            cfg.Save();
        }

        int remove = -1;
        for (int i = 0; i < _pack!.Slides.Count; i++)
        {
            var s = _pack.Slides[i];
            ImGui.PushID($"slide{i}");
            if (ImGui.Selectable($"{s.Name}##sel", _slideIdx == i))
            {
                _slideIdx  = i;
                _branchIdx = s.Branches.Count > 0 ? 0 : -1;
            }
            if (ImGui.BeginPopupContextItem($"sctx{i}"))
            {
                if (ImGui.MenuItem("Delete step")) remove = i;
                ImGui.EndPopup();
            }
            ImGui.PopID();
        }

        if (remove >= 0)
        {
            _pack.Slides.RemoveAt(remove);
            _slideIdx  = _pack.Slides.Count > 0 ? 0 : -1;
            _branchIdx = _slideIdx >= 0 && _pack.Slides[0].Branches.Count > 0 ? 0 : -1;
            cfg.Save();
        }
    }

    private void DrawTrigger(Configuration cfg, StratSlide slide)
    {
        float scale = ImGuiHelpers.GlobalScale;
        ImGui.TextColored(Ui.Gold, "Trigger");

        ImGui.SetNextItemWidth(200f * scale);
        string nm = slide.Name;
        if (ImGui.InputText("name##slide", ref nm, 64)) { slide.Name = nm; cfg.Save(); }

        int onIdx = Array.IndexOf(OnVals, slide.On);
        if (onIdx < 0) onIdx = OnVals.Length - 1;
        ImGui.SetNextItemWidth(160f * scale);
        if (ImGui.Combo("on##slide", ref onIdx, OnNames, OnNames.Length))
        { slide.On = OnVals[onIdx]; cfg.Save(); }

        bool byId = slide.MatchById;
        if (ImGui.Checkbox("match by id##slide", ref byId)) { slide.MatchById = byId; cfg.Save(); }

        if (slide.MatchById)
        {
            int id = (int)slide.DataId;
            ImGui.SetNextItemWidth(140f * scale);
            if (ImGui.InputInt("action id##slide", ref id))
            { slide.DataId = (uint)Math.Max(0, id); cfg.Save(); }
        }
        else
        {
            ImGui.SetNextItemWidth(200f * scale);
            string pat = slide.Pattern;
            if (ImGui.InputText("name contains##slide", ref pat, 96))
            { slide.Pattern = pat; cfg.Save(); }
        }

        float delay = slide.DelaySeconds;
        ImGui.SetNextItemWidth(140f * scale);
        if (ImGui.DragFloat("delay (s)##slide", ref delay, 0.1f, 0f, 30f))
        { slide.DelaySeconds = MathF.Max(0f, delay); cfg.Save(); }
    }

    private void DrawBranches(Configuration cfg, StratSlide slide)
    {
        float scale = ImGuiHelpers.GlobalScale;
        ImGui.TextColored(Ui.Gold, "Branches");
        ImGui.SameLine();
        if (ImGui.SmallButton("+ Branch"))
        {
            slide.Branches.Add(new StratBranch { Name = $"Variant {slide.Branches.Count + 1}" });
            _branchIdx = slide.Branches.Count - 1;
            cfg.Save();
        }

        int remove = -1;
        for (int i = 0; i < slide.Branches.Count; i++)
        {
            ImGui.PushID($"branch{i}");
            if (ImGui.Selectable($"{slide.Branches[i].Name}##b", _branchIdx == i))
                _branchIdx = i;
            if (ImGui.BeginPopupContextItem($"bctx{i}"))
            {
                if (ImGui.MenuItem("Delete branch")) remove = i;
                ImGui.EndPopup();
            }
            ImGui.PopID();
        }
        if (remove >= 0)
        {
            slide.Branches.RemoveAt(remove);
            _branchIdx = slide.Branches.Count > 0 ? 0 : -1;
            cfg.Save();
        }

        var branch = (_branchIdx >= 0 && _branchIdx < slide.Branches.Count) ? slide.Branches[_branchIdx] : null;
        if (branch == null) return;

        ImGui.Spacing();
        ImGui.SetNextItemWidth(200f * scale);
        string bn = branch.Name;
        if (ImGui.InputText("branch name##b", ref bn, 64)) { branch.Name = bn; cfg.Save(); }

        ImGui.TextColored(Ui.Gold, "Use this variant when");
        ImGui.SameLine();
        if (ImGui.SmallButton("+ Condition"))
        {
            branch.Conditions.Add(new StratCondition());
            cfg.Save();
        }

        if (branch.Conditions.Count > 1)
        {
            int mode = branch.RequireAll ? 0 : 1;
            ImGui.SameLine();
            ImGui.SetNextItemWidth(110f * scale);
            if (ImGui.Combo("##matchmode", ref mode, new[] { "match ALL", "match ANY" }, 2))
            { branch.RequireAll = mode == 0; cfg.Save(); }
        }

        if (branch.Conditions.Count == 0)
            ImGui.TextColored(Ui.Dimmed, "No conditions = default / catch-all. Keep this branch last.");

        int condRemove = -1;
        for (int ci = 0; ci < branch.Conditions.Count; ci++)
        {
            ImGui.PushID($"cond{ci}");
            if (DrawCondition(cfg, branch.Conditions[ci], ci)) condRemove = ci;
            ImGui.PopID();
        }
        if (condRemove >= 0) { branch.Conditions.RemoveAt(condRemove); cfg.Save(); }
    }

    private bool DrawCondition(Configuration cfg, StratCondition c, int idx)
    {
        float scale = ImGuiHelpers.GlobalScale;
        bool remove = false;

        if (ImGui.SmallButton("x")) remove = true;
        ImGui.SameLine();

        int kind = (int)c.Kind;
        ImGui.SetNextItemWidth(140f * scale);
        if (ImGui.Combo("##ck", ref kind, CondKindNames, CondKindNames.Length))
        { c.Kind = (CondKind)kind; cfg.Save(); }

        ImGui.SameLine();
        int neg = c.Negate ? 1 : 0;
        ImGui.SetNextItemWidth(70f * scale);
        if (ImGui.Combo("##cneg", ref neg, new[] { "is", "is NOT" }, 2))
        { c.Negate = neg == 1; cfg.Save(); }

        ImGui.SameLine();
        switch (c.Kind)
        {
            case CondKind.MyStatus:
            {
                ImGui.SetNextItemWidth(150f * scale);
                string sn = c.StatusName;
                if (ImGui.InputText("##csn", ref sn, 64)) { c.StatusName = sn; cfg.Save(); }
                ImGui.SameLine();
                int sid = (int)c.StatusId;
                ImGui.SetNextItemWidth(90f * scale);
                if (ImGui.InputInt("id##csid", ref sid, 0)) { c.StatusId = (uint)Math.Max(0, sid); cfg.Save(); }
                ImGui.SameLine();
                if (ImGui.SmallButton("Pick")) ImGui.OpenPopup($"##cpick{idx}");
                if (c.StatusId != 0)
                {
                    ImGui.SameLine();
                    ImGui.TextColored(SelfHasStatus(c.StatusId) ? Ui.Green : Ui.Dimmed,
                        SelfHasStatus(c.StatusId) ? "on you" : "not on you");
                }
                DrawCondStatusPicker(cfg, c, $"##cpick{idx}");
                break;
            }
            case CondKind.MyRole:
            {
                int role = (int)c.Role;
                ImGui.SetNextItemWidth(120f * scale);
                if (ImGui.Combo("##crole", ref role, RoleCatNames, RoleCatNames.Length))
                { c.Role = (RoleCat)role; cfg.Save(); }
                break;
            }
            case CondKind.BossSide:
            {
                int side = (int)c.BossSide;
                ImGui.SetNextItemWidth(70f * scale);
                if (ImGui.Combo("##cside", ref side, CompassNames, CompassNames.Length))
                { c.BossSide = (Compass)side; cfg.Save(); }
                ImGui.SameLine();
                if (ImGui.SmallButton("Use target"))
                {
                    var tgt = Plugin.ObjectTable.LocalPlayer?.TargetObject;
                    if (tgt != null) { c.BossId = tgt.EntityId; c.BossName = tgt.Name.TextValue; cfg.Save(); }
                }
                if (!string.IsNullOrEmpty(c.BossName))
                { ImGui.SameLine(); ImGui.TextColored(Ui.Dimmed, c.BossName); }
                break;
            }
            case CondKind.TetherOnMe:
            {
                ImGui.SetNextItemWidth(130f * scale);
                string tn = c.TetherName;
                if (ImGui.InputText("##ctn", ref tn, 48)) { c.TetherName = tn; cfg.Save(); }
                ImGui.SameLine();
                int tid = (int)c.TetherId;
                ImGui.SetNextItemWidth(90f * scale);
                if (ImGui.InputInt("id##ctid", ref tid, 0)) { c.TetherId = (uint)Math.Max(0, tid); cfg.Save(); }
                ImGui.SameLine();
                if (ImGui.SmallButton("Pick"))
                {
                    var me = Plugin.ObjectTable.LocalPlayer;
                    if (me != null)
                        foreach (var t in _plugin.Capture.ActiveTethers)
                            if (t.From == me.EntityId || t.To == me.EntityId)
                            { c.TetherId = t.Id; cfg.Save(); break; }
                }
                if (c.TetherId != 0)
                { ImGui.SameLine(); ImGui.TextColored(Ui.Dimmed, $"#{c.TetherId}"); }
                break;
            }
        }
        return remove;
    }

    private void DrawCondStatusPicker(Configuration cfg, StratCondition c, string popupId)
    {
        if (!ImGui.BeginPopup(popupId)) return;
        ImGui.TextDisabled("Your current statuses — click to bind");
        ImGui.Separator();

        var me = Plugin.ObjectTable.LocalPlayer;
        bool any = false;
        if (me != null)
        {
            foreach (var s in me.StatusList)
            {
                if (s == null || s.StatusId == 0) continue;
                any = true;
                string nm = StatusName(s.StatusId);
                if (ImGui.Selectable($"{nm}  (#{s.StatusId})##cst{s.StatusId}"))
                {
                    c.StatusId   = s.StatusId;
                    c.StatusName = nm;
                    cfg.Save();
                    ImGui.CloseCurrentPopup();
                }
            }
        }
        if (!any) ImGui.TextColored(Ui.Dimmed, "No statuses on you right now.");
        ImGui.EndPopup();
    }

    private static bool SelfHasStatus(uint statusId)
    {
        var me = Plugin.ObjectTable.LocalPlayer;
        if (me == null || statusId == 0) return false;
        foreach (var s in me.StatusList)
            if (s != null && s.StatusId == statusId) return true;
        return false;
    }

    private static string StatusName(uint id)
    {
        if (id == 0) return "";
        var st = Plugin.DataManager.GetExcelSheet<Lumina.Excel.Sheets.Status>().GetRowOrDefault(id);
        var n = st?.Name.ExtractText();
        return string.IsNullOrEmpty(n) ? $"#{id}" : n;
    }

    private void DrawArenaPane(Configuration cfg, StratSlide slide, StratBranch branch, float scale)
    {
        ImGui.TextColored(Ui.Dimmed, "Pick a role, then click the arena to drop your spot.");

        for (int i = 0; i < RoleNames.Length; i++)
        {
            var role = (StratRole)i;
            bool has = branch.Spots.Exists(s => s.Role == role && s.Enabled);
            bool sel = _placing == role;
            if (sel) ImGui.PushStyleColor(ImGuiCol.Button, Ui.Accent with { W = 0.85f });
            else if (has) ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0.30f, 0.40f, 0.30f, 1f));
            if (ImGui.Button($"{RoleNames[i]}##role", new Vector2(40f * scale, 0))) _placing = role;
            if (sel || has) ImGui.PopStyleColor();
            if (i < RoleNames.Length - 1) ImGui.SameLine();
        }

        var spot = branch.Spots.Find(s => s.Role == _placing);

        ImGui.Spacing();
        if (spot != null)
        {
            bool en = spot.Enabled;
            if (Ui.ToggleSwitch("##spoten", ref en)) { spot.Enabled = en; cfg.Save(); }
            ImGui.SameLine(0, 8f);
            ImGui.AlignTextToFramePadding();
            ImGui.TextColored(en ? new Vector4(1f, 1f, 1f, 1f) : Ui.Dimmed, $"{RoleNames[(int)_placing]} spot");

            int sh = Array.IndexOf(ShapeVals, spot.Shape);
            if (sh < 0) sh = 0;
            ImGui.SameLine(0, 16f);
            ImGui.SetNextItemWidth(110f * scale);
            if (ImGui.Combo("##spshape", ref sh, ShapeNames, ShapeNames.Length)) { spot.Shape = ShapeVals[sh]; cfg.Save(); }

            ImGui.SameLine();
            ImGui.SetNextItemWidth(120f * scale);
            float r = spot.Radius;
            if (ImGui.DragFloat("radius##sp", ref r, 0.1f, 0.3f, 30f)) { spot.Radius = r; cfg.Save(); }

            var col = spot.Color;
            if (ImGui.ColorEdit4("marker##sp", ref col, ImGuiColorEditFlags.NoInputs | ImGuiColorEditFlags.AlphaBar)) { spot.Color = col; cfg.Save(); }

            ImGui.SameLine();
            bool leash = spot.ShowLeash;
            if (ImGui.Checkbox("leash##sp", ref leash)) { spot.ShowLeash = leash; cfg.Save(); }
            if (spot.ShowLeash)
            {
                ImGui.SameLine();
                var lc = spot.LeashColor;
                if (ImGui.ColorEdit4("line##sp", ref lc, ImGuiColorEditFlags.NoInputs | ImGuiColorEditFlags.AlphaBar)) { spot.LeashColor = lc; cfg.Save(); }
            }

            ImGui.SameLine();
            ImGui.SetNextItemWidth(110f * scale);
            float dur = spot.Duration;
            if (ImGui.DragFloat("hold (s)##sp", ref dur, 0.2f, 1f, 60f)) { spot.Duration = dur; cfg.Save(); }

            int anchor = (int)spot.Anchor;
            ImGui.SetNextItemWidth(170f * scale);
            if (ImGui.Combo("##spanchor", ref anchor, new[] { "Fixed arena spot", "My tether (clone)" }, 2))
            { spot.Anchor = (SpotAnchor)anchor; cfg.Save(); }

            if (spot.Anchor == SpotAnchor.TetheredToMe)
            {
                ImGui.SameLine();
                int tid = (int)spot.TetherId;
                ImGui.SetNextItemWidth(90f * scale);
                if (ImGui.InputInt("tether id##sp", ref tid, 0)) { spot.TetherId = (uint)Math.Max(0, tid); cfg.Save(); }
                ImGui.SameLine();
                if (ImGui.SmallButton("Pick##sptether"))
                {
                    var me = Plugin.ObjectTable.LocalPlayer;
                    if (me != null)
                        foreach (var t in _plugin.Capture.ActiveTethers)
                            if (t.From == me.EntityId || t.To == me.EntityId)
                            { spot.TetherId = t.Id; cfg.Save(); break; }
                }
                ImGui.SameLine();
                ImGui.TextColored(Ui.Dimmed, spot.TetherId == 0 ? "(any tether on me)" : $"#{spot.TetherId}");
            }
        }
        else
        {
            ImGui.TextColored(Ui.Dimmed, $"{RoleNames[(int)_placing]} has no spot yet — click the arena.");
        }

        ImGui.Spacing();
        ImGui.Checkbox("Snap 1y", ref _snap);
        ImGui.SameLine();
        if (ImGui.Button("Test in game")) _plugin.Strat.Preview(slide, branch);
        ImGui.SameLine();
        if (ImGui.Button("Clear shapes")) _plugin.Host.CleanVfx();
        ImGui.SameLine();
        if (ImGui.Button("Recenter on me")) _canvas.RecenterOnPlayer();
        ImGui.SameLine();
        if (ImGui.Button("Center on target"))
        {
            var tgt = Plugin.ObjectTable.LocalPlayer?.TargetObject;
            if (tgt != null) { _canvas.CenterX = tgt.Position.X; _canvas.CenterZ = tgt.Position.Z; }
        }

        ImGui.SameLine(0, 16f);
        ImGui.AlignTextToFramePadding();
        ImGui.TextDisabled("Zoom");
        ImGui.SameLine();
        ImGui.SetNextItemWidth(120f * scale);
        ImGui.SliderFloat("##stratzoom", ref _canvas.ViewRadius, 5f, _canvas.MaxRadius, "%.0fy", ImGuiSliderFlags.Logarithmic);
        ImGui.SameLine();
        ImGui.Checkbox("Map", ref _canvas.ShowGameMap);
        ImGui.SameLine();
        ImGui.Checkbox("Names", ref _canvas.ShowNames);

        ImGui.Spacing();

        float size = MathF.Min(ImGui.GetContentRegionAvail().X, ImGui.GetContentRegionAvail().Y) - 4f;
        if (size < 200f) size = 200f;

        var f = _canvas.Begin("##stratpad", size);
        _canvas.DrawArenaFloor(f, _pack!.ArenaShape, _pack.ArenaRadius, _pack.ArenaCenterX, _pack.ArenaCenterZ);
        _canvas.DrawLiveActors(f);

        var dl = f.Dl;
        uint white = ImGui.ColorConvertFloat4ToU32(new Vector4(1f, 1f, 1f, 0.95f));
        uint black = ImGui.ColorConvertFloat4ToU32(new Vector4(0.05f, 0.05f, 0.05f, 1f));

        foreach (var s in branch.Spots)
        {
            if (!s.Enabled) continue;
            var sp = _canvas.ToScreen(s.Position.X, s.Position.Z, f.Origin, f.Size);
            uint c  = ImGui.ColorConvertFloat4ToU32(s.Color with { W = 1f });
            bool isSel = s.Role == _placing;

            if (s.ShowLeash)
            {
                var meObj = Plugin.ObjectTable.LocalPlayer;
                if (meObj != null)
                {
                    var msp = _canvas.ToScreen(meObj.Position.X, meObj.Position.Z, f.Origin, f.Size);
                    dl.AddLine(msp, sp, ImGui.ColorConvertFloat4ToU32(s.LeashColor), 1.6f);
                }
            }

            dl.AddCircleFilled(sp, isSel ? 9f : 7f, c);
            dl.AddCircle(sp, isSel ? 12f : 9f, isSel ? white : ImGui.ColorConvertFloat4ToU32(new Vector4(0f, 0f, 0f, 0.6f)), 18, isSel ? 2f : 1.2f);

            var lbl = RoleNames[(int)s.Role];
            var ts  = ImGui.CalcTextSize(lbl);
            dl.AddText(new Vector2(sp.X - ts.X * 0.5f, sp.Y - ts.Y * 0.5f), black, lbl);
        }

        _canvas.End(f);

        if (f.Active && ImGui.IsMouseDown(ImGuiMouseButton.Left))
        {
            var w = _canvas.ToWorld(ImGui.GetMousePos(), f.Origin, f.Size);
            if (_snap) { w.X = MathF.Round(w.X); w.Y = MathF.Round(w.Y); }
            else { w.X = MathF.Round(w.X, 1); w.Y = MathF.Round(w.Y, 1); }
            var target = branch.Spots.Find(s => s.Role == _placing);
            if (target == null)
            {
                target = new RoleSpot { Role = _placing };
                branch.Spots.Add(target);
            }
            target.Position = new Vector3(w.X, 0f, w.Y);
            target.Enabled  = true;
            cfg.Save();
        }
    }
}
