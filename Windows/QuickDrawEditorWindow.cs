using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Windowing;
using YapYapDraw.Logging;
using YapYapDraw.QuickDraws;

namespace YapYapDraw.Windows;

public sealed class QuickDrawEditorWindow : Window, IDisposable
{
    private readonly Plugin _plugin;
    private QuickDrawDef?    _t;
    private QuickDrawDef?    _real;
    private QuickDrawModule? _owner;
    private bool   _isNew;
    private bool   _dirty;
    private int    _sel = -1;
    private string _status = "";
    private string _librarySearch = "";
    private string _zoneSearch = "";
    private readonly Dictionary<string, string> _condSearch = new();
    private readonly Dictionary<uint, Dalamud.Interface.Textures.ISharedImmediateTexture> _iconCache = new();

    // Armed ground picker: next world-click sets the spot (handled on framework tick).
    private Action<Vector3>? _groundPick;
    private bool _wasLmbDown;
    private bool _wasEscDown;
    private int  _groundPickGrace;
    private bool _padSnapGrid;

    public QuickDrawEditorWindow(Plugin plugin)
        : base("Edit Quick Draw###YapYapDrawEditor")
    {
        _plugin = plugin;
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(720, 360),
            MaximumSize = new Vector2(1600, 1400),
        };
        Size          = new Vector2(1000, 720);
        SizeCondition = ImGuiCond.FirstUseEver;
    }

    public void Dispose() { }

    public override void PreDraw()  => Ui.PushTheme();
    public override void PostDraw() => Ui.PopTheme();

    public void Open(QuickDrawDef t)
    {
        _real   = t;
        _owner  = FindOwner(t);
        _t      = t.Clone();
        _isNew  = false;
        _dirty  = false;
        _sel    = -1;
        _status = "";
        IsOpen  = true;
    }

    public void OpenFor(LogEvent e)
    {
        bool onMe = e.TargetId == Plugin.PlayerState.EntityId;
        bool onlySelf = (e.IsStatus || e.Kind == LogKind.Headmarker) && onMe;
        var source = e.IsStatus
            ? SourceFilter.Anyone
            : e.SourceKind switch
            {
                ActorKind.Enemy => SourceFilter.Enemy,
                ActorKind.You   => SourceFilter.You,
                ActorKind.Party => SourceFilter.Party,
                _               => SourceFilter.Anyone,
            };

        var targetRole = onMe ? RoleFilter.Any : RoleOf(e.TargetId);
        uint zone = Plugin.ClientState.TerritoryType;

        var on = e.Kind switch
        {
            LogKind.CastStart  => TriggerMatch.Cast,
            LogKind.StatusGain => TriggerMatch.StatusGain,
            LogKind.StatusLose => TriggerMatch.StatusLose,
            LogKind.Death      => TriggerMatch.Death,
            LogKind.Headmarker => TriggerMatch.Headmarker,
            LogKind.Tether     => TriggerMatch.Tether,
            _                  => TriggerMatch.Any,
        };

        // Sensible default anchor: a status/marker on a player sticks to that
        // player; a boss cast sits where the boss is.
        var anchor = (e.IsStatus || e.Kind is LogKind.Headmarker)
            ? (onMe ? DrawAnchor.Self : DrawAnchor.Target)
            : DrawAnchor.Source;

        // A VFX log line carries an omen path, not a matchable event — grab the path
        // into a Custom look and let the user pick a real trigger.
        bool isVfx = e.Kind == LogKind.Vfx;
        var draw = new DrawSpec { Anchor = anchor };
        if (isVfx)
            draw.CustomVfx = e.Name;

        _t = new QuickDrawDef
        {
            Name       = e.Name,
            Pattern    = isVfx ? "" : e.Name,
            On         = isVfx ? TriggerMatch.Any : on,
            Source     = source,
            OnlyOnSelf = onlySelf,
            TargetRole = targetRole,
            MatchById  = !isVfx && e.DataId != 0,
            DataId     = isVfx ? 0u : e.DataId,
            IconId     = e.IconId,
            AnyZone    = zone == 0,
            Zones      = zone != 0 ? new List<uint> { zone } : new(),
            Draw       = draw,
        };

        _real   = null;
        _owner  = _plugin.Configuration.QuickModule();
        _isNew  = true;
        _dirty  = true;
        _sel    = -1;
        _status = "";
        IsOpen  = true;
    }

    public void OpenForCatalog(FightCatalog.Entry entry, uint territory)
    {
        var on = entry.Kind switch
        {
            FightCatalog.Kind.Cast       => TriggerMatch.Cast,
            FightCatalog.Kind.Status     => TriggerMatch.StatusGain,
            FightCatalog.Kind.Headmarker => TriggerMatch.Headmarker,
            FightCatalog.Kind.Tether     => TriggerMatch.Tether,
            _                            => TriggerMatch.Cast,
        };

        var anchor = entry.Kind switch
        {
            FightCatalog.Kind.Cast => DrawAnchor.Source,
            _                      => DrawAnchor.Target,
        };

        _t = new QuickDrawDef
        {
            Name       = entry.Name,
            Pattern    = entry.Name,
            On         = on,
            Source     = on == TriggerMatch.Cast ? SourceFilter.Enemy : SourceFilter.Anyone,
            OnlyOnSelf = on is TriggerMatch.StatusGain or TriggerMatch.Headmarker or TriggerMatch.Tether,
            MatchById  = entry.Id != 0,
            DataId     = entry.Id,
            IconId     = entry.Icon,
            AnyZone    = territory == 0,
            Zones      = territory != 0 ? new List<uint> { territory } : new(),
            Draw       = new DrawSpec { Anchor = anchor },
        };

        _real   = null;
        _owner  = _plugin.Configuration.QuickModule();
        _isNew  = true;
        _dirty  = true;
        _sel    = -1;
        _status = "";
        IsOpen  = true;
    }

    private QuickDrawModule? FindOwner(QuickDrawDef t)
    {
        foreach (var m in _plugin.Configuration.QuickDrawModules)
            if (m.Draws.Contains(t)) return m;
        return null;
    }

    private void Commit()
    {
        var cfg = _plugin.Configuration;
        if (_t == null) return;

        if (_isNew)
        {
            _owner ??= cfg.QuickModule();
            _owner.Draws.Add(_t);
            _isNew = false;
        }
        else if (_real != null && _owner != null)
        {
            int i = _owner.Draws.IndexOf(_real);
            if (i >= 0) _owner.Draws[i] = _t;
            else _owner.Draws.Add(_t);
        }
        else
        {
            (_owner ?? cfg.QuickModule()).Draws.Add(_t);
        }

        cfg.Save();

        _real   = _t;
        _t      = _t.Clone();
        _dirty  = false;
        _status = "Saved";
    }

    public override void Draw()
    {
        if (_t is null) { ImGui.TextDisabled("No quick draw selected."); return; }
        var t   = _t;
        var cfg = _plugin.Configuration;

        ProcessGroundPick();

        Ui.NavBar(_plugin, "");

        ImGui.AlignTextToFramePadding();
        ImGui.TextColored(Ui.Gold, "Quick Draw");
        ImGui.SameLine();
        ImGui.SetNextItemWidth(-1);
        string name = t.Name;
        if (ImGui.InputTextWithHint("##name", "draw name", ref name, 64)) { t.Name = name; _dirty = true; }

        ImGui.SetNextItemWidth(200f * ImGuiHelpers.GlobalScale);
        string group = t.Group;
        if (ImGui.InputTextWithHint("Section (group)", "optional, e.g. Phase 2", ref group, 48))
        { t.Group = group; _dirty = true; }

        if (_sel >= t.FollowUps.Count) _sel = -1;

        float scale  = ImGuiHelpers.GlobalScale;
        float footer = ImGui.GetFrameHeightWithSpacing() * 2f + 12f * scale;
        float bodyH  = ImGui.GetContentRegionAvail().Y - footer;
        if (bodyH < 120f * scale) bodyH = 120f * scale;

        float leftW = ImGui.GetContentRegionAvail().X * 0.54f;

        if (ImGui.BeginChild("##cfg", new Vector2(leftW, bodyH), true))
        {
            Banner("WHEN IT FIRES", "the moment to react to");
            DrawMatch(t, cfg);

            Banner("WHERE IT WORKS", "limit it to one duty — optional");
            DrawZones(t);

            Banner("THEN…", "chain more draws — click one to edit its shape →");
            DrawStepList(t, cfg);

            ImGui.Spacing();
            ImGui.SetNextItemOpen(false, ImGuiCond.Appearing);
            if (ImGui.CollapsingHeader("ADVANCED — numbers, filters, remember-a-value, anti-spam"))
                DrawAdvanced(t, cfg);
        }
        ImGui.EndChild();

        ImGui.SameLine();

        if (ImGui.BeginChild("##editor", new Vector2(0, bodyH), true))
            DrawShapeColumn(t, cfg);
        ImGui.EndChild();

        ImGui.Spacing();
        ImGui.Separator();

        ImGui.BeginDisabled(!_dirty);
        if (ImGui.Button(_isNew ? "Save draw" : "Save changes")) Commit();
        ImGui.EndDisabled();
        ImGui.SameLine();
        if (ImGui.Button("Test")) _plugin.Engine.Preview(t);
        ImGui.SameLine();
        if (ImGui.Button("Cancel")) IsOpen = false;
        ImGui.SameLine();
        if (ImGui.Button("Copy share code"))
        {
            ImGui.SetClipboardText(ShareCodec.Encode(ShareCodec.DrawPrefix, t));
            _status = "Share code copied";
        }
        ImGui.SameLine();
        if (ImGui.Button("Paste code")) PasteCode();

        if (_dirty) { ImGui.SameLine(); ImGui.TextColored(Ui.Gold, "● unsaved"); }
        else if (!string.IsNullOrEmpty(_status)) { ImGui.SameLine(); ImGui.TextColored(Ui.Blue, _status); }
    }

    private void DrawShapeColumn(QuickDrawDef t, Configuration cfg)
    {
        Banner("WHAT IT DRAWS", "the shape on the floor");
        DrawShapeEditor("main", t.Draw, () => t.DrawEnabled, v => t.DrawEnabled = v);

        for (int i = 0; i < t.FollowUps.Count; i++)
        {
            var s = t.FollowUps[i];
            ImGui.Spacing();
            ImGui.Separator();
            ImGui.Spacing();

            Banner($"FOLLOW-UP #{i + 1} DRAWS", StepSummary(s, i + 1));
            DrawShapeEditor(s.Id, s.Draw, () => s.DrawEnabled, v => s.DrawEnabled = v);
        }
    }

    private static readonly string[] ShapeNames  = { "Circle", "Donut", "Fan", "Rectangle", "Line", "Tower", "Knockback", "Laser" };
    private static readonly string[] AnchorNames = { "On the caster", "On the target", "On me", "Fixed spot", "Where it happened" };
    private static readonly string[] LinkNames   = { "Event target", "Event caster", "My target", "Nearest player", "Nearest enemy", "Player w/ same debuff", "Fixed spot" };

    private static readonly (string Name, Vector4 Color)[] ColorPresets =
    {
        ("Meteor / spread (orange)", new(1f,    0.55f, 0.10f, 0.32f)),
        ("Cone / bait (yellow)",     new(1f,    0.80f, 0.10f, 0.30f)),
        ("Stack (green)",            new(0.10f, 0.75f, 0.40f, 0.50f)),
        ("Tower / grab (green)",     new(0.20f, 0.95f, 0.35f, 0.60f)),
        ("Safe (cyan)",              new(0.20f, 0.90f, 1f,    0.50f)),
        ("Mechanic (violet)",        new(0.45f, 0.40f, 1f,    0.55f)),
        ("Dark (purple)",            new(0.6f,  0f,    1f,    0.55f)),
        ("Danger (red)",             new(0.96f, 0.20f, 0.20f, 0.55f)),
    };

    private void DrawShapeEditor(string id, DrawSpec d, Func<bool> getEnabled, Action<bool> setEnabled)
    {
        ImGui.PushID(id);
        float scale = ImGuiHelpers.GlobalScale;

        bool on = getEnabled();
        if (ImGui.Checkbox("Draw a shape", ref on)) { setEnabled(on); _dirty = true; }

        if (getEnabled())
        {
            ImGui.Indent(8f * scale);

            ImGui.AlignTextToFramePadding();
            ImGui.TextColored(Ui.Dimmed, "Shape");
            int shape = (int)d.Shape;
            if (StratUI.SegmentedBar(ShapeNames, ref shape))
            {
                var next = (QuickShape)shape;
                if (next == QuickShape.Line && d.Shape != QuickShape.Line)
                {
                    if (d.HalfWidth > 1.5f) d.HalfWidth = 0.5f;
                    d.Link = LinkTarget.FixedSpot;
                }
                d.Shape = next;
                _dirty = true;
            }

            var color = d.Color;
            if (ImGui.ColorEdit4("##col", ref color, ImGuiColorEditFlags.NoInputs | ImGuiColorEditFlags.AlphaPreview))
            { d.Color = color; _dirty = true; }
            ImGui.SameLine(); ImGui.AlignTextToFramePadding(); ImGui.TextColored(Ui.Dimmed, "colour");

            ImGui.SameLine(0, 14f);
            ImGui.AlignTextToFramePadding();
            ImGui.TextColored(Ui.Dimmed, "module presets:");
            foreach (var (name, swatch) in ColorPresets)
            {
                ImGui.SameLine(0, 4f);
                if (ImGui.ColorButton($"{name}##sw", swatch,
                        ImGuiColorEditFlags.NoTooltip | ImGuiColorEditFlags.AlphaPreview,
                        new Vector2(18f * scale, 18f * scale)))
                { d.Color = swatch; _dirty = true; }
                if (ImGui.IsItemHovered()) ImGui.SetTooltip(name);
            }
            DragF("Transparency", () => d.Color.W, v => { d.Color = d.Color with { W = v }; }, 0.01f, 0.05f, 1f, "%.2f");

            ImGui.Spacing();
            DrawShapeDims(d, scale);

            ImGui.Spacing();
            DrawPlacement(d, scale);

            if (d.Shape == QuickShape.Line)
                DrawLinkPicker(d, scale);

            DrawOffsets(d);

            ImGui.Spacing();
            bool useEvt = d.UseEventDuration;
            if (ImGui.Checkbox("Match the cast / debuff time", ref useEvt))
            { d.UseEventDuration = useEvt; _dirty = true; }
            if (!d.UseEventDuration)
                DragF("Seconds on floor", () => d.Duration, v => d.Duration = v, 0.1f, 0.2f, 120f, "%.1fs");

            ImGui.Unindent(8f * scale);
        }

        ImGui.PopID();
    }

    private void DrawShapeDims(DrawSpec d, float scale)
    {
        switch (d.Shape)
        {
            case QuickShape.Circle:
                DragF("Radius (y)", () => d.Radius, v => d.Radius = v, 0.1f, 0.5f, 60f);
                break;
            case QuickShape.Donut:
                DragF("Inner radius (y)", () => d.InnerRadius, v => d.InnerRadius = v, 0.1f, 0f, 60f);
                DragF("Outer radius (y)", () => d.Radius, v => d.Radius = v, 0.1f, 0.5f, 60f);
                break;
            case QuickShape.Fan:
                DragF("Length (y)", () => d.Radius, v => d.Radius = v, 0.1f, 0.5f, 60f);
                DragI("Angle (°)", () => d.FanAngle, v => d.FanAngle = v, 1, 5, 360);
                DrawFacing(d);
                break;
            case QuickShape.Rectangle:
                DragF("Length (y)", () => d.Length, v => d.Length = v, 0.1f, 0.5f, 100f);
                DragF("Half-width (y)", () => d.HalfWidth, v => d.HalfWidth = v, 0.1f, 0.5f, 60f);
                DrawFacing(d);
                break;
            case QuickShape.Line:
                DragF("Half-width (y)", () => d.HalfWidth, v => d.HalfWidth = v, 0.1f, 0.2f, 30f);
                ImGui.TextColored(Ui.Dimmed, "Length stretches automatically to the far end.");
                break;
            case QuickShape.Tower:
                DragF("Radius (y)", () => d.Radius, v => d.Radius = v, 0.1f, 0.5f, 30f);
                ImGui.TextColored(Ui.Dimmed, "Stand-here soak marker.");
                break;
            case QuickShape.Knockback:
                DragF("Radius (y)", () => d.Radius, v => d.Radius = v, 0.1f, 0.5f, 60f);
                DrawFacing(d);
                break;
            case QuickShape.Laser:
                DragF("Length (y)", () => d.Length, v => d.Length = v, 0.1f, 0.5f, 100f);
                DragF("Half-width (y)", () => d.HalfWidth, v => d.HalfWidth = v, 0.1f, 0.2f, 60f);
                DrawFacing(d);
                break;
        }
    }

    private void DrawFacing(DrawSpec d)
    {
        bool spin = d.OrientToFacing;
        if (ImGui.Checkbox("Spin with the actor's facing", ref spin)) { d.OrientToFacing = spin; _dirty = true; }
        if (ImGui.IsItemHovered())
            ImGui.SetTooltip("On: the shape turns as the actor (you, the caster, the target) turns.\nThe angle below becomes an offset from straight-ahead.\nOff: the angle is a fixed compass bearing (0 = north).");
        DragF(d.OrientToFacing ? "Offset from facing (°)" : "Facing (°)",
            () => d.Rotation, v => d.Rotation = v, 1f, -360f, 360f);
    }

    private void DrawOffsets(DrawSpec d)
    {
        ImGui.Spacing();
        ImGui.TextColored(Ui.Dimmed, "Nudge (relative to facing when spinning, else world):");
        DragF("Forward (y)", () => d.OffsetForward, v => d.OffsetForward = v, 0.1f, -40f, 40f);
        ImGui.SameLine();
        DragF("Side (y)", () => d.OffsetSide, v => d.OffsetSide = v, 0.1f, -40f, 40f);
    }

    private void DrawPlacement(DrawSpec d, float scale)
    {
        ImGui.AlignTextToFramePadding();
        ImGui.TextColored(Ui.Dimmed, "Place it");
        int anchor = (int)d.Anchor;
        ImGui.SetNextItemWidth(220f * scale);
        if (ImGui.Combo("##anchor", ref anchor, AnchorNames, AnchorNames.Length))
        { d.Anchor = (DrawAnchor)anchor; _dirty = true; }

        if (d.Anchor == DrawAnchor.FixedPosition)
            DrawSpotPicker("anchor spot", () => d.FixedPosition, p => { d.FixedPosition = p; _dirty = true; }, scale);
        else if (d.Anchor is DrawAnchor.Source or DrawAnchor.Target or DrawAnchor.Self)
        {
            bool stick = d.AttachToActor;
            if (ImGui.Checkbox("Stick to the actor (follows them)", ref stick))
            { d.AttachToActor = stick; _dirty = true; }
        }
    }

    private void DrawLinkPicker(DrawSpec d, float scale)
    {
        ImGui.Spacing();
        ImGui.AlignTextToFramePadding();
        ImGui.TextColored(Ui.Dimmed, "Connect to");
        int link = (int)d.Link;
        ImGui.SetNextItemWidth(220f * scale);
        if (ImGui.Combo("##link", ref link, LinkNames, LinkNames.Length))
        { d.Link = (LinkTarget)link; _dirty = true; }

        if (d.Link == LinkTarget.FixedSpot)
            DrawSpotPicker("far end", () => d.LinkPosition, p => { d.LinkPosition = p; _dirty = true; }, scale);
    }

    // Clicks on the game world never reach ImGui while the editor is open, so pick
    // on the framework tick with raw cursor coords + ScreenToWorld.
    public void TickGroundPick()
    {
        if (_groundPick == null)
        {
            IsClickthrough = false;
            _wasLmbDown = false;
            _wasEscDown = false;
            _groundPickGrace = 0;
            return;
        }

        IsClickthrough = true;

        bool esc = (GetAsyncKeyState(0x1B) & 0x8000) != 0;
        if (esc && !_wasEscDown) { _groundPick = null; _wasEscDown = esc; return; }
        _wasEscDown = esc;

        bool lmb = (GetAsyncKeyState(0x01) & 0x8000) != 0;
        if (_groundPickGrace > 0)
        {
            _groundPickGrace--;
            _wasLmbDown = lmb;
            return;
        }

        if (lmb && !_wasLmbDown && GetCursorPos(out var pt))
        {
            var screen = new Vector2(pt.X, pt.Y);
            if (Plugin.GameGui.ScreenToWorld(screen, out var world))
            {
                _groundPick(new Vector3(MathF.Round(world.X, 2), 0f, MathF.Round(world.Z, 2)));
                _dirty = true;
            }
            _groundPick = null;
        }
        _wasLmbDown = lmb;
    }

    private void ProcessGroundPick()
    {
        if (_groundPick == null) return;

        var fg = ImGui.GetForegroundDrawList();
        var m  = ImGui.GetMousePos();
        fg.AddCircle(m, 9f, ImGui.ColorConvertFloat4ToU32(Ui.Accent), 16, 2f);
        fg.AddText(new Vector2(m.X + 14f, m.Y + 2f),
            ImGui.ColorConvertFloat4ToU32(Ui.Gold), "click the ground  (Esc to cancel)");
    }

    private void DrawSpotPicker(string id, Func<Vector3> get, Action<Vector3> set, float scale)
    {
        ImGui.PushID(id);

        bool armed = _groundPick != null;
        if (armed) ImGui.PushStyleColor(ImGuiCol.Button, Ui.Accent with { W = 0.9f });
        if (ImGui.SmallButton(armed ? "Picking…" : "Pick on ground"))
        {
            if (armed) _groundPick = null;
            else { _groundPick = set; _groundPickGrace = 3; }
        }
        if (armed) ImGui.PopStyleColor();
        if (ImGui.IsItemHovered())
            ImGui.SetTooltip("Click here, then click a spot on the ground in-game to grab its coordinates.");

        ArenaPad.Draw(id, _plugin, get, set, scale, _padSnapGrid, v => _padSnapGrid = v, () => _dirty = true);
        ImGui.PopID();
    }

    [DllImport("user32.dll")]
    private static extern short GetAsyncKeyState(int vKey);

    [DllImport("user32.dll")]
    private static extern bool GetCursorPos(out Point pt);

    [StructLayout(LayoutKind.Sequential)]
    private struct Point
    {
        public int X;
        public int Y;
    }

    private bool DragF(string label, Func<float> get, Action<float> set, float step, float min, float max, string fmt = "%.1f")
    {
        float v = get();
        ImGui.SetNextItemWidth(160f * ImGuiHelpers.GlobalScale);
        if (ImGui.DragFloat(label, ref v, step, min, max, fmt, ImGuiSliderFlags.AlwaysClamp))
        { set(v); _dirty = true; return true; }
        return false;
    }

    private bool DragI(string label, Func<int> get, Action<int> set, int step, int min, int max)
    {
        int v = get();
        ImGui.SetNextItemWidth(160f * ImGuiHelpers.GlobalScale);
        if (ImGui.DragInt(label, ref v, step, min, max))
        { set(Math.Clamp(v, min, max)); _dirty = true; return true; }
        return false;
    }

    private readonly Dictionary<uint, Dalamud.Interface.Textures.ISharedImmediateTexture?> _iconPreview = new();

    private void DrawIconPreview(uint iconId, float size)
    {
        if (!_iconPreview.TryGetValue(iconId, out var tex))
        {
            tex = Plugin.TextureProvider.GetFromGameIcon(new Dalamud.Interface.Textures.GameIconLookup(iconId));
            _iconPreview[iconId] = tex;
        }
        var wrap = tex?.GetWrapOrDefault();
        if (wrap != null) ImGui.Image(wrap.Handle, new Vector2(size, size));
        else ImGui.Dummy(new Vector2(size, size));
    }

    private static void Sub(string label)
    {
        ImGui.Spacing();
        ImGui.TextColored(Ui.Blue, label);
        ImGui.Separator();
    }

    private void DrawAdvanced(QuickDrawDef t, Configuration cfg)
    {
        float scale = ImGuiHelpers.GlobalScale;

        Sub("Only draw when a number matches");
        ImGui.TextColored(Ui.Dimmed, "e.g. only draw if the stack count is 3, or the caster is below 20% HP. All rows must pass.");
        int rmNum = -1;
        for (int i = 0; i < t.NumConds.Count; i++)
        {
            var c = t.NumConds[i];
            ImGui.PushID($"num{i}");
            int f = (int)c.Field;
            ImGui.SetNextItemWidth(155f * scale);
            if (ImGui.Combo("##f", ref f, NumFieldNames, NumFieldNames.Length)) { c.Field = (NumField)f; _dirty = true; }
            ImGui.SameLine();
            int op = (int)c.Op;
            ImGui.SetNextItemWidth(95f * scale);
            if (ImGui.Combo("##op", ref op, NumOpNames, NumOpNames.Length)) { c.Op = (NumOp)op; _dirty = true; }
            ImGui.SameLine();
            float v = c.Value;
            ImGui.SetNextItemWidth(90f * scale);
            if (ImGui.InputFloat("##v", ref v)) { c.Value = v; _dirty = true; }
            ImGui.SameLine();
            if (ImGui.SmallButton("remove##n")) rmNum = i;
            ImGui.PopID();
        }
        if (rmNum >= 0) { t.NumConds.RemoveAt(rmNum); _dirty = true; }
        if (ImGui.SmallButton("+ add a number rule")) { t.NumConds.Add(new NumCond()); _dirty = true; }

        Sub("Remove the shape early");
        ImGui.TextColored(Ui.Dimmed, "Wipe this draw the moment a matching event lands (e.g. clear the telegraph as soon as the cast goes off).");
        var clr = t.ClearOn;
        bool clrOn = clr.Enabled;
        if (ImGui.Checkbox("Clear the shape when…", ref clrOn)) { clr.Enabled = clrOn; _dirty = true; }
        if (clr.Enabled)
        {
            ImGui.Indent(8f * scale);
            int evi = (int)clr.On - 1; if (evi < 0) evi = 0;
            ImGui.SetNextItemWidth(160f * scale);
            if (ImGui.Combo("##clrev", ref evi, ClearEventNames, ClearEventNames.Length))
            { clr.On = (FollowUpOn)(evi + 1); _dirty = true; }
            ImGui.SameLine();
            string cp = clr.Pattern;
            ImGui.SetNextItemWidth(180f * scale);
            if (ImGui.InputTextWithHint("##clrpat", "name (blank = same as above)", ref cp, 128))
            { clr.Pattern = cp; clr.MatchById = false; _dirty = true; }
            bool cself = clr.OnlyOnSelf;
            if (ImGui.Checkbox("only when it's on me", ref cself)) { clr.OnlyOnSelf = cself; _dirty = true; }
            ImGui.SameLine();
            float cw = clr.Seconds;
            ImGui.SetNextItemWidth(120f * scale);
            if (ImGui.DragFloat("within (s)", ref cw, 0.5f, 1f, 120f, "%.0fs", ImGuiSliderFlags.AlwaysClamp))
            { clr.Seconds = cw; _dirty = true; }
            ImGui.Unindent(8f * scale);
        }

        Sub("Remember a value (advanced)");
        ImGui.TextColored(Ui.Dimmed, "A note this draw writes down, that ANOTHER draw can read later to decide where to draw.");
        ImGui.TextColored(Ui.Blue, "   1.  on the \"Bomb\" debuff  →  save   bomb = {target}");
        ImGui.TextColored(Ui.Blue, "   2.  on the \"Tower\" cast   →  only draw if   bomb  is  {target}");

        ImGui.Spacing();
        ImGui.Text("When this draws, save a note:");
        int rmSet = -1;
        for (int i = 0; i < t.SetVars.Count; i++)
        {
            var a = t.SetVars[i];
            ImGui.PushID($"set{i}");
            ImGui.AlignTextToFramePadding(); ImGui.TextColored(Ui.Dimmed, "call it");
            ImGui.SameLine();
            string nm = a.Name;
            ImGui.SetNextItemWidth(110f * scale);
            if (ImGui.InputTextWithHint("##n", "bomb", ref nm, 32)) { a.Name = nm; _dirty = true; }
            ImGui.SameLine();
            int op = (int)a.Op;
            ImGui.SetNextItemWidth(95f * scale);
            if (ImGui.Combo("##sop", ref op, VarOpNames, VarOpNames.Length)) { a.Op = (VarOp)op; _dirty = true; }
            ImGui.SameLine();
            string val = a.Value;
            ImGui.SetNextItemWidth(150f * scale);
            if (ImGui.InputTextWithHint("##sv", "{target}", ref val, 64)) { a.Value = val; _dirty = true; }
            ImGui.SameLine();
            if (ImGui.SmallButton("remove##s")) rmSet = i;
            ImGui.PopID();
        }
        if (rmSet >= 0) { t.SetVars.RemoveAt(rmSet); _dirty = true; }
        if (ImGui.SmallButton("+ save a note")) { t.SetVars.Add(new VarAction { Value = "{target}" }); _dirty = true; }

        ImGui.Spacing();
        ImGui.Text("Only draw if a saved note matches:");
        int rmVc = -1;
        for (int i = 0; i < t.VarConds.Count; i++)
        {
            var c = t.VarConds[i];
            ImGui.PushID($"vc{i}");
            ImGui.AlignTextToFramePadding(); ImGui.TextColored(Ui.Dimmed, "the note");
            ImGui.SameLine();
            string nm = c.Name;
            ImGui.SetNextItemWidth(110f * scale);
            if (ImGui.InputTextWithHint("##vn", "bomb", ref nm, 32)) { c.Name = nm; _dirty = true; }
            ImGui.SameLine();
            int op = (int)c.Op;
            ImGui.SetNextItemWidth(95f * scale);
            if (ImGui.Combo("##vop", ref op, NumOpNames, NumOpNames.Length)) { c.Op = (NumOp)op; _dirty = true; }
            ImGui.SameLine();
            string val = c.Value;
            ImGui.SetNextItemWidth(150f * scale);
            if (ImGui.InputTextWithHint("##vv", "{target}", ref val, 64)) { c.Value = val; _dirty = true; }
            ImGui.SameLine();
            bool numeric = c.Numeric;
            if (ImGui.Checkbox("123", ref numeric)) { c.Numeric = numeric; _dirty = true; }
            if (ImGui.IsItemHovered()) ImGui.SetTooltip("compare as numbers instead of text");
            ImGui.SameLine();
            if (ImGui.SmallButton("remove##v")) rmVc = i;
            ImGui.PopID();
        }
        if (rmVc >= 0) { t.VarConds.RemoveAt(rmVc); _dirty = true; }
        if (ImGui.SmallButton("+ require a note")) { t.VarConds.Add(new VarCond { Value = "{target}" }); _dirty = true; }

        Sub("Stop it spamming");
        float cd = t.Cooldown;
        if (TimeDrag("Cooldown (0 = default)", ref cd, 120f, 170f)) { t.Cooldown = cd; _dirty = true; }

        ImGui.TextColored(Ui.Dimmed, "If it draws again while the shape is still on the floor:");
        int mode = (int)(t.NoReentry && t.Concurrency == Concurrency.Stack ? Concurrency.Wait : t.Concurrency);
        ImGui.SetNextItemWidth(260f * scale);
        if (ImGui.Combo("##conc", ref mode, ConcurrencyNames, ConcurrencyNames.Length))
        { t.Concurrency = (Concurrency)mode; t.NoReentry = t.Concurrency == Concurrency.Wait; _dirty = true; }
    }

    private void PasteCode()
    {
        var code = ImGui.GetClipboardText();
        if (_t != null && ShareCodec.TryDecode<QuickDrawDef>(ShareCodec.DrawPrefix, code, out var trig) && trig != null)
        {
            trig.Id = _t.Id;
            _t = trig;
            _dirty = true;
            _status = "Loaded from code";
        }
        else _status = "Clipboard isn't a quick-draw code";
    }

    private void DrawMatch(QuickDrawDef t, Configuration cfg)
    {
        int onIdx = (int)t.On;
        if (Combo("Event", MatchNames, ref onIdx)) { t.On = (TriggerMatch)onIdx; _dirty = true; }

        if (t.On == TriggerMatch.Chat)
        {
            ImGui.SameLine();
            ImGui.TextDisabled("fires on a chat / battle-log line");

            string cpat = t.Pattern;
            ImGui.SetNextItemWidth(-1);
            if (ImGui.InputTextWithHint("##chatpat", "text or regex to match in the line", ref cpat, 256))
            { t.Pattern = cpat; _dirty = true; }
            ImGui.SameLine();
            bool crx = t.UseRegex;
            if (ImGui.Checkbox("regex", ref crx)) { t.UseRegex = crx; _dirty = true; }

            float cdelay = t.DelaySeconds;
            if (TimeDrag("Delay before drawing", ref cdelay, 60f, 220f)) { t.DelaySeconds = cdelay; _dirty = true; }
            return;
        }

        if (t.On is TriggerMatch.Headmarker or TriggerMatch.Tether)
        {
            int id = (int)t.DataId;
            ImGui.SetNextItemWidth(160f * ImGuiHelpers.GlobalScale);
            string label = t.On == TriggerMatch.Headmarker ? "Marker id (0 = any)" : "Tether id (0 = any)";
            if (ImGui.InputInt(label, ref id))
            { t.DataId = (uint)Math.Max(0, id); t.MatchById = t.DataId != 0; _dirty = true; }
        }
        else
        {
            DrawPicker(t);
        }

        DrawWho(t);

        float delay = t.DelaySeconds;
        if (TimeDrag("Delay before drawing", ref delay, 60f, 220f)) { t.DelaySeconds = delay; _dirty = true; }
    }

    private static RoleFilter RoleOf(uint actorId)
    {
        if (actorId == 0) return RoleFilter.Any;
        try
        {
            if (Plugin.ObjectTable.SearchById(actorId)
                    is Dalamud.Game.ClientState.Objects.Types.IBattleChara bc && bc.ClassJob.IsValid)
                return bc.ClassJob.Value.Role switch
                {
                    1      => RoleFilter.Tank,
                    4      => RoleFilter.Healer,
                    2 or 3 => RoleFilter.Dps,
                    _      => RoleFilter.Any,
                };
        }
        catch { }
        return RoleFilter.Any;
    }

    private void DrawWho(QuickDrawDef t)
    {
        float scale = ImGuiHelpers.GlobalScale;
        bool  tether = t.On == TriggerMatch.Tether;

        ImGui.AlignTextToFramePadding();
        ImGui.TextColored(Ui.Gold, "From");
        ImGui.SameLine();
        int srcIdx = (int)t.Source;
        ImGui.SetNextItemWidth(120f * scale);
        if (ImGui.Combo("##src", ref srcIdx, SourceNames, SourceNames.Length))
        { t.Source = (SourceFilter)srcIdx; _dirty = true; }
        ImGui.SameLine();
        ImGui.AlignTextToFramePadding();
        ImGui.TextColored(Ui.Dimmed, "who's a");
        ImGui.SameLine();
        int srIdx = (int)t.SourceRole;
        ImGui.SetNextItemWidth(95f * scale);
        if (ImGui.Combo("##srole", ref srIdx, RoleNames, RoleNames.Length))
        { t.SourceRole = (RoleFilter)srIdx; _dirty = true; }
        ImGui.SameLine();
        ImGui.TextDisabled("(the caster / applier)");

        ImGui.AlignTextToFramePadding();
        ImGui.TextColored(Ui.Gold, "To  ");
        ImGui.SameLine();
        int toIdx = t.OnlyOnSelf ? 1 : t.TargetRole switch
        {
            RoleFilter.Tank   => 2,
            RoleFilter.Healer => 3,
            RoleFilter.Dps    => 4,
            _                 => 0,
        };
        ImGui.SetNextItemWidth(120f * scale);
        if (ImGui.Combo("##to", ref toIdx, ToNames, ToNames.Length))
        {
            t.OnlyOnSelf = toIdx == 1;
            t.TargetRole = toIdx switch
            {
                2 => RoleFilter.Tank,
                3 => RoleFilter.Healer,
                4 => RoleFilter.Dps,
                _ => RoleFilter.Any,
            };
            _dirty = true;
        }
        ImGui.SameLine();
        ImGui.TextDisabled(tether ? "(either end of the tether)" : "(who it lands on)");
    }

    private static bool TimeDrag(string label, ref float secs, float max, float width = 130f)
    {
        ImGui.SetNextItemWidth(width * ImGuiHelpers.GlobalScale);
        return ImGui.DragFloat(label, ref secs, 0.1f, 0f, max, "%.1fs", ImGuiSliderFlags.AlwaysClamp);
    }

    private void DrawZones(QuickDrawDef t)
    {
        float scale = ImGuiHelpers.GlobalScale;
        uint cur = Plugin.ClientState.TerritoryType;

        bool anyZone = t.AnyZone;
        if (ImGui.Checkbox("Works in any zone", ref anyZone)) { t.AnyZone = anyZone; _dirty = true; }

        if (t.AnyZone)
        {
            ImGui.TextDisabled("This draw fires everywhere.");
            return;
        }

        if (ImGui.Button("+ Add current zone"))
        {
            if (cur != 0 && !t.Zones.Contains(cur)) { t.Zones.Add(cur); _dirty = true; }
        }
        ImGui.SameLine();
        ImGui.TextDisabled($"you're in: {ZoneLibrary.NameOf(cur)}");

        ImGui.SetNextItemWidth(260f * scale);
        ImGui.InputTextWithHint("##zoneq", "type a duty / zone name…", ref _zoneSearch, 64);
        if (!string.IsNullOrWhiteSpace(_zoneSearch) &&
            ImGui.BeginChild("##zoneres", new Vector2(0, 120f * scale), true))
        {
            foreach (var z in ZoneLibrary.Search(_zoneSearch))
            {
                if (ImGui.Selectable($"{z.Name}"))
                {
                    if (!t.Zones.Contains(z.TerritoryId)) { t.Zones.Add(z.TerritoryId); _dirty = true; }
                    _zoneSearch = "";
                }
            }
            ImGui.EndChild();
        }

        if (t.Zones.Count == 0)
        {
            ImGui.TextColored(new Vector4(1f, 0.6f, 0.3f, 1f),
                "No zones picked — add one, or enable \"any zone\".");
            return;
        }

        uint removeZone = 0;
        foreach (var z in t.Zones)
        {
            ImGui.BulletText(ZoneLibrary.NameOf(z));
            ImGui.SameLine();
            if (ImGui.SmallButton($"remove##z{z}")) removeZone = z;
        }
        if (removeZone != 0) { t.Zones.Remove(removeZone); _dirty = true; }
    }

    private void DrawStepList(QuickDrawDef t, Configuration cfg)
    {
        float scale = ImGuiHelpers.GlobalScale;

        FollowUpStep? remove = null;
        for (int i = 0; i < t.FollowUps.Count; i++)
        {
            var s = t.FollowUps[i];
            ImGui.PushID($"fu{s.Id}");

            if (ImGui.SmallButton("✕")) remove = s;
            ImGui.SameLine();
            bool selected = _sel == i;
            if (ImGui.Selectable(StepSummary(s, i + 1), selected)) _sel = i;

            if (selected)
            {
                ImGui.Indent(12f * scale);
                DrawStepConfig(s);
                ImGui.Unindent(12f * scale);
                ImGui.Spacing();
            }

            ImGui.PopID();
        }

        if (remove != null)
        {
            int idx = t.FollowUps.IndexOf(remove);
            t.FollowUps.Remove(remove);
            _dirty = true;
            if (_sel == idx) _sel = -1;
            else if (_sel > idx) _sel--;
        }

        ImGui.Spacing();
        ImGui.PushStyleColor(ImGuiCol.Button, Ui.Accent with { W = 0.85f });
        if (ImGui.Button("✚  Add follow-up", new Vector2(-1, 0)))
        {
            t.FollowUps.Add(new FollowUpStep());
            _sel = t.FollowUps.Count - 1;
            _dirty = true;
        }
        ImGui.PopStyleColor();
    }

    private static string StepSummary(FollowUpStep s, int n)
    {
        string when = s.On switch
        {
            FollowUpOn.Timer      => $"after {s.Seconds:0.#}s",
            FollowUpOn.Cast       => "on a cast",
            FollowUpOn.StatusGain => "on status gained",
            FollowUpOn.StatusLose => "on status lost",
            FollowUpOn.Headmarker => "on headmarker",
            FollowUpOn.Tether     => "on tether",
            FollowUpOn.Death      => "on death",
            FollowUpOn.Chat       => "on chat line",
            _                     => "follow-up",
        };
        return $"#{n}  {when}  →  {s.Draw.Shape}";
    }

    private void DrawStepConfig(FollowUpStep s)
    {
        float scale = ImGuiHelpers.GlobalScale;

        int on = (int)s.On;
        ImGui.SetNextItemWidth(200f * scale);
        if (ImGui.Combo("React to", ref on, FollowUpNames, FollowUpNames.Length))
        { s.On = (FollowUpOn)on; s.Conditions.Clear(); _dirty = true; }

        float secs = s.Seconds;
        string secLabel = s.On == FollowUpOn.Timer ? "Wait (seconds)" : "Within (seconds)";
        if (TimeDrag(secLabel, ref secs, 180f, 120f)) { s.Seconds = secs; _dirty = true; }

        if (s.On != FollowUpOn.Timer)
        {
            s.EnsureConditions();
            DrawConditions(s);
        }
    }

    private void DrawConditions(FollowUpStep s)
    {
        float scale = ImGuiHelpers.GlobalScale;
        bool markerKind = s.On is FollowUpOn.Headmarker or FollowUpOn.Tether;

        if (s.Conditions.Count > 1)
        {
            ImGui.AlignTextToFramePadding();
            ImGui.TextDisabled("Match");
            ImGui.SameLine();
            int mode = s.RequireAll ? 0 : 1;
            ImGui.SetNextItemWidth(150f * scale);
            if (ImGui.Combo("##mode", ref mode, MatchModeNames, MatchModeNames.Length))
            { s.RequireAll = mode == 0; _dirty = true; }
            ImGui.SameLine();
            ImGui.TextDisabled(s.RequireAll ? "need all (within the window)" : "any one fires it");
        }

        bool chatKind = s.On == FollowUpOn.Chat;

        int removeAt = -1;
        for (int i = 0; i < s.Conditions.Count; i++)
        {
            var c = s.Conditions[i];
            ImGui.PushID($"c{i}");

            if (chatKind)
            {
                string pat = c.Pattern;
                ImGui.SetNextItemWidth(260f * scale);
                if (ImGui.InputTextWithHint("##pat", "text or regex in the line", ref pat, 256))
                { c.Pattern = pat; _dirty = true; }
                ImGui.SameLine();
                bool rx = c.UseRegex;
                if (ImGui.Checkbox("regex", ref rx)) { c.UseRegex = rx; _dirty = true; }
            }
            else if (markerKind)
            {
                int id = (int)c.DataId;
                ImGui.SetNextItemWidth(130f * scale);
                if (ImGui.InputInt("id (0=any)", ref id)) { c.DataId = (uint)Math.Max(0, id); _dirty = true; }
            }
            else
            {
                string pat = c.Pattern;
                ImGui.SetNextItemWidth(200f * scale);
                string hint = s.On == FollowUpOn.Death ? "who (blank=any)" : "name (blank=any)";
                if (ImGui.InputTextWithHint("##pat", hint, ref pat, 64))
                { c.Pattern = pat; c.MatchById = false; _dirty = true; }
                if (c.MatchById && c.DataId != 0) { ImGui.SameLine(); ImGui.TextDisabled($"#{c.DataId}"); }
            }

            if (!chatKind)
            {
                ImGui.SameLine();
                bool self = c.OnlyOnSelf;
                string lbl = s.On == FollowUpOn.Tether ? "me" : s.On == FollowUpOn.Death ? "I die" : "on me";
                if (ImGui.Checkbox(lbl, ref self)) { c.OnlyOnSelf = self; _dirty = true; }
            }

            if (s.On is FollowUpOn.Cast or FollowUpOn.StatusGain or FollowUpOn.StatusLose)
            {
                ImGui.SameLine();
                DrawCondLibrary(s, i, c);
            }

            if (s.Conditions.Count > 1)
            {
                ImGui.SameLine();
                if (ImGui.SmallButton("x")) removeAt = i;
            }

            if (!chatKind && !markerKind)
                DrawCondWho(c);

            ImGui.PopID();
        }
        if (removeAt >= 0) { s.Conditions.RemoveAt(removeAt); _dirty = true; }

        if (ImGui.SmallButton("+ condition")) { s.Conditions.Add(new FollowCond()); _dirty = true; }
        if (!markerKind)
        {
            ImGui.SameLine();
            ImGui.TextDisabled(chatKind ? "add another line to require together"
                                        : "add a 2nd debuff/cast to require together");
        }
    }

    private void DrawCondWho(FollowCond c)
    {
        float scale = ImGuiHelpers.GlobalScale;
        ImGui.Indent(14f * scale);

        ImGui.AlignTextToFramePadding();
        ImGui.TextColored(Ui.Dimmed, "from");
        ImGui.SameLine();
        int src = (int)c.Source;
        ImGui.SetNextItemWidth(110f * scale);
        if (ImGui.Combo("##fsrc", ref src, SourceNames, SourceNames.Length)) { c.Source = (SourceFilter)src; _dirty = true; }
        ImGui.SameLine();
        int sr = (int)c.SourceRole;
        ImGui.SetNextItemWidth(85f * scale);
        if (ImGui.Combo("##fsrole", ref sr, RoleNames, RoleNames.Length)) { c.SourceRole = (RoleFilter)sr; _dirty = true; }
        ImGui.SameLine(0, 14f * scale);
        ImGui.AlignTextToFramePadding();
        ImGui.TextColored(Ui.Dimmed, "to");
        ImGui.SameLine();
        int tr = (int)c.TargetRole;
        ImGui.SetNextItemWidth(85f * scale);
        if (ImGui.Combo("##ftrole", ref tr, RoleNames, RoleNames.Length)) { c.TargetRole = (RoleFilter)tr; _dirty = true; }

        ImGui.Unindent(14f * scale);
    }

    private static void Banner(string title, string hint)
    {
        float scale = ImGuiHelpers.GlobalScale;
        ImGui.Spacing();
        ImGui.Spacing();

        var draw = ImGui.GetWindowDrawList();
        var p0   = ImGui.GetCursorScreenPos();
        float h  = ImGui.GetTextLineHeight();

        draw.AddRectFilled(p0, new Vector2(p0.X + 3f * scale, p0.Y + h),
            ImGui.ColorConvertFloat4ToU32(Ui.Accent), 1f);

        ImGui.Dummy(new Vector2(9f * scale, h));
        ImGui.SameLine();
        ImGui.TextColored(new Vector4(0.95f, 0.93f, 0.93f, 1f), title);
        ImGui.SameLine();
        ImGui.TextColored(Ui.Dimmed, "  " + hint);
        ImGui.Separator();
        ImGui.Spacing();
    }

    private void DrawCondLibrary(FollowUpStep s, int i, FollowCond c)
    {
        float scale = ImGuiHelpers.GlobalScale;
        string key = s.Id + ":" + i;

        if (ImGui.SmallButton("Find")) ImGui.OpenPopup($"find{key}");
        if (!ImGui.BeginPopup($"find{key}")) return;

        if (!_condSearch.TryGetValue(key, out var q)) q = "";
        ImGui.SetNextItemWidth(240f * scale);
        if (ImGui.InputTextWithHint("##q", "status / ability name…", ref q, 64)) _condSearch[key] = q;

        bool wantStatus = s.On is FollowUpOn.StatusGain or FollowUpOn.StatusLose;
        bool wantAction = s.On == FollowUpOn.Cast;
        if (!string.IsNullOrWhiteSpace(q) &&
            ImGui.BeginChild("##res", new Vector2(280f * scale, 180f * scale), true))
        {
            foreach (var r in GameLibrary.Search(q))
            {
                if (wantStatus && !r.IsStatus) continue;
                if (wantAction && r.IsStatus) continue;
                ImGui.PushID($"{(r.IsStatus ? "s" : "a")}{r.Id}");
                DrawIcon(r.Icon, ImGui.GetTextLineHeight());
                ImGui.SameLine();
                if (ImGui.Selectable($"{r.Name}  ({(r.IsStatus ? "status" : "action")} #{r.Id})"))
                {
                    c.Pattern   = r.Name;
                    c.DataId    = r.Id;
                    c.MatchById = true;
                    _dirty = true;
                    ImGui.CloseCurrentPopup();
                }
                ImGui.PopID();
            }
            ImGui.EndChild();
        }
        ImGui.EndPopup();
    }

    private void DrawPicker(QuickDrawDef t)
    {
        float scale = ImGuiHelpers.GlobalScale;
        bool isStatus = t.On is TriggerMatch.StatusGain or TriggerMatch.StatusLose;
        string kind = isStatus ? "status" : "ability";

        if (t.MatchById && t.DataId != 0)
        {
            float h = ImGui.GetFrameHeight();
            if (t.IconId != 0) DrawIconPreview(t.IconId, h);
            else                DrawIcon(0, h);
            ImGui.SameLine();
            ImGui.AlignTextToFramePadding();
            ImGui.TextColored(new Vector4(0.95f, 0.93f, 0.93f, 1f),
                string.IsNullOrWhiteSpace(t.Pattern) ? "(unnamed)" : t.Pattern);
            ImGui.SameLine();
            ImGui.AlignTextToFramePadding();
            ImGui.TextColored(Ui.Dimmed, $"{kind} #{t.DataId}");
        }
        else
        {
            ImGui.AlignTextToFramePadding();
            ImGui.TextColored(Ui.Dimmed, "No ability / status picked yet —");
        }

        ImGui.SameLine();
        if (ImGui.Button("Find…")) ImGui.OpenPopup("##findmain");
        ImGui.SameLine();
        bool byId = t.MatchById;
        if (ImGui.Checkbox("match by exact id", ref byId)) { t.MatchById = byId; _dirty = true; }

        DrawPickerPopup(t);

        if (!t.MatchById)
        {
            string pattern = t.Pattern;
            ImGui.SetNextItemWidth(240f * scale);
            if (ImGui.InputTextWithHint("Name contains", "e.g. Heavenly Hell", ref pattern, 128))
            { t.Pattern = pattern; _dirty = true; }
            ImGui.SameLine();
            bool rx = t.UseRegex;
            if (ImGui.Checkbox("regex", ref rx)) { t.UseRegex = rx; _dirty = true; }
        }
    }

    private void DrawPickerPopup(QuickDrawDef t)
    {
        float scale = ImGuiHelpers.GlobalScale;
        if (!ImGui.BeginPopup("##findmain")) return;

        ImGui.SetNextItemWidth(280f * scale);
        ImGui.InputTextWithHint("##lib", "type an ability or status name…", ref _librarySearch, 64);

        if (!string.IsNullOrWhiteSpace(_librarySearch) &&
            ImGui.BeginChild("##libresults", new Vector2(320f * scale, 220f * scale), true))
        {
            foreach (var r in GameLibrary.Search(_librarySearch))
            {
                ImGui.PushID($"{(r.IsStatus ? "s" : "a")}{r.Id}");
                DrawIcon(r.Icon, ImGui.GetTextLineHeight());
                ImGui.SameLine();
                if (ImGui.Selectable($"{r.Name}  ({(r.IsStatus ? "status" : "action")} #{r.Id})"))
                {
                    t.On        = r.IsStatus ? TriggerMatch.StatusGain : TriggerMatch.Cast;
                    t.MatchById = true;
                    t.DataId    = r.Id;
                    t.Pattern   = r.Name;
                    t.IconId    = r.Icon;
                    if (string.IsNullOrWhiteSpace(t.Name) || t.Name == "New quick draw") t.Name = r.Name;
                    _dirty = true;
                    ImGui.CloseCurrentPopup();
                }
                ImGui.PopID();
            }
            ImGui.EndChild();
        }
        ImGui.EndPopup();
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

    private static readonly string[] MatchNames  = { "Anything", "Cast started", "Status gained", "Status lost", "Death", "Headmarker", "Tether", "Chat / battle-log", "Cast ended (snapshot)" };
    private static readonly string[] SourceNames = { "Anyone", "Boss / enemy", "You", "Party" };
    private static readonly string[] RoleNames   = { "Any", "Tank", "Healer", "DPS" };
    private static readonly string[] ToNames     = { "Anyone", "Me", "a Tank", "a Healer", "a DPS" };
    private static readonly string[] NumFieldNames = { "Stack count", "Value (cast/skill)", "Caster HP %", "Target HP %", "Param 1", "Param 2", "Param 3", "Param 4" };
    private static readonly string[] NumOpNames  = { "is", "is not", "less than", "at most", "more than", "at least" };
    private static readonly string[] VarOpNames  = { "set to", "add" };
    private static readonly string[] ConcurrencyNames =
        { "Wait — keep the first shape", "Replace — newest shape wins", "Stack — show both" };
    private static readonly string[] MatchModeNames = { "all of", "any of" };
    // Maps to FollowUpOn values Cast..Chat (skips Timer at index 0).
    private static readonly string[] ClearEventNames =
        { "a cast starts", "a status is gained", "a status is lost", "a headmarker appears", "a tether forms", "someone dies", "chat says", "the cast resolves" };
    private static readonly string[] FollowUpNames =
    {
        "Wait, then draw",
        "When a cast starts",
        "When a status is gained",
        "When a status is lost",
        "When a headmarker appears",
        "When a tether forms",
        "When someone dies",
        "When chat says…",
        "When the cast resolves",
    };

    private static bool Combo(string label, string[] names, ref int index)
    {
        ImGui.SetNextItemWidth(180f * ImGuiHelpers.GlobalScale);
        return ImGui.Combo(label, ref index, names, names.Length);
    }
}
