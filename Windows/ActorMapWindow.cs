using System;
using System.Collections.Generic;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Interface.Textures;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Windowing;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using YapYapDraw.Logging;
using LuminaAction = Lumina.Excel.Sheets.Action;
using LuminaTerritory = Lumina.Excel.Sheets.TerritoryType;

namespace YapYapDraw.Windows;

public sealed class ActorMapWindow : Window, IDisposable
{
    private const float CenterX = 100f;
    private const float CenterZ = 100f;
    private const float ReplayFade = 6f;

    private static readonly Vector4 ColYou    = new(0.35f, 0.85f, 1f,    1f);
    private static readonly Vector4 ColParty  = new(0.40f, 0.85f, 0.50f, 1f);
    private static readonly Vector4 ColEnemy  = new(0.96f, 0.42f, 0.42f, 1f);
    private static readonly Vector4 ColAlly   = new(0.55f, 0.90f, 0.70f, 1f);
    private static readonly Vector4 ColPet    = new(0.70f, 0.85f, 0.55f, 1f);
    private static readonly Vector4 ColObject = new(0.66f, 0.66f, 0.70f, 1f);
    private static readonly Vector4 ColId     = new(0.60f, 0.70f, 0.85f, 1f);

    private enum MapCategory : byte { You, Party, Enemy, Ally, Pet, Object }

    private readonly Plugin _plugin;
    private readonly Dictionary<uint, ISharedImmediateTexture> _iconCache = new();
    private readonly List<MapActor> _liveActors = new();

    private float  _viewRadius = 40f;
    private float  _maxRadius  = 200f;
    private float  _centerX = CenterX;
    private float  _centerZ = CenterZ;
    private bool   _dragging;
    private uint   _lastTerr = uint.MaxValue;

    private uint  _mapId = uint.MaxValue;
    private ISharedImmediateTexture? _mapTex;
    private float _mapScale = 1f;
    private float _mapOffX;
    private float _mapOffZ;
    private bool   _replayMode;
    private uint   _replayTerr;
    private uint   _replayMapId;
    private string _search = "";
    private uint   _selectedId;
    private uint   _ctxId;

    private static Configuration Cfg => Plugin.Config;

    // FieldMarkers order is A B C D 1 2 3 4; the game icons aren't contiguous.
    private static readonly uint[] WaymarkIcons =
        { 61241, 61242, 61243, 61247, 61244, 61245, 61246, 61248 };

    private int      _replayPull;
    private int      _cachedPull = -1;
    private bool     _playing;
    private double   _playT;
    private float    _playSpeed = 1f;
    private DateTime _lastFrame = DateTime.Now;
    private DateTime _pullStart;
    private double   _pullDuration = 1;
    private readonly List<LogEvent> _pullEvents = new();
    private readonly List<CombatLogCapture.MapFrame> _pullFrames = new();

    public ActorMapWindow(Plugin plugin)
        : base("YapYap Live Map###YapYapMap")
    {
        _plugin = plugin;
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(560, 420),
            MaximumSize = new Vector2(2400, 2000),
        };
    }

    public void Dispose() { }

    public override void PreDraw()  => Ui.PushTheme();
    public override void PostDraw() => Ui.PopTheme();

    public override void Draw()
    {
        var now = DateTime.Now;
        double dt = (now - _lastFrame).TotalSeconds;
        _lastFrame = now;

        uint terr = Plugin.ClientState.TerritoryType;
        if (terr != _lastTerr) { _lastTerr = terr; RecenterView(); }

        DrawTopBar();

        if (_replayMode)
        {
            try
            {
                EnsurePullCache();
                AdvancePlay(dt);
                DrawReplayControls();
            }
            catch (Exception ex) { ImGui.TextDisabled($"replay error: {ex.Message}"); }
        }

        ImGui.Separator();

        var   avail   = ImGui.GetContentRegionAvail();
        float spacing = ImGui.GetStyle().ItemSpacing.X;
        float scale   = ImGuiHelpers.GlobalScale;

        // Both panes live in their own fixed-width child so their widths sum to the
        // available space exactly. The square radar is clipped inside its pane, so it
        // can never overflow and shove the side panel onto the next line (that was
        // the disappearing / bottom-shifting sidebar bug, triggered by UI scale).
        float minList = MathF.Min(240f * scale, avail.X * 0.5f);
        float maxList = MathF.Max(minList, avail.X - 220f);
        float listW   = Math.Clamp(avail.X * 0.32f, minList, maxList);
        float canvasW = MathF.Max(120f, avail.X - listW - spacing);
        float h       = MathF.Max(160f, avail.Y);

        const ImGuiWindowFlags paneFlags = ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse;
        if (ImGui.BeginChild("##mappane", new Vector2(canvasW, h), false, paneFlags))
        {
            float side = MathF.Max(120f, MathF.Min(canvasW, h));
            try { DrawCanvas(side); }
            catch (Exception ex)
            {
                ImGui.Dummy(new Vector2(side, side));
                ImGui.GetWindowDrawList().AddText(
                    ImGui.GetItemRectMin(),
                    ImGui.ColorConvertFloat4ToU32(ColEnemy), $"map error: {ex.Message}");
            }
        }
        ImGui.EndChild();

        ImGui.SameLine();

        if (ImGui.BeginChild("##sidepanel", new Vector2(listW, h), true))
        {
            try
            {
                if (_replayMode) DrawReplayListBody();
                else             DrawActorListBody();
            }
            catch (Exception ex) { ImGui.TextDisabled($"list error: {ex.Message}"); }
        }
        ImGui.EndChild();

        DrawActorContextPopup();
    }

    // ---- top bar ----------------------------------------------------------------

    private void DrawTopBar()
    {
        if (ModeButton("Live", !_replayMode)) _replayMode = false;
        ImGui.SameLine();
        if (ModeButton("Replay", _replayMode))
        {
            if (!_replayMode) JumpToLatestPull();
            _replayMode = true;
        }

        ImGui.SameLine();
        ImGui.TextDisabled("|");
        ImGui.SameLine();

        ImGui.AlignTextToFramePadding();
        ImGui.TextDisabled("Zoom");
        ImGui.SameLine();
        ImGui.SetNextItemWidth(120f * ImGuiHelpers.GlobalScale);
        ImGui.SliderFloat("##zoom", ref _viewRadius, 5f, _maxRadius, "%.0fy", ImGuiSliderFlags.Logarithmic);
        if (ImGui.IsItemHovered())
            ImGui.SetTooltip("Scroll the map to zoom toward the cursor · drag to pan.");

        ImGui.SameLine();
        if (ImGui.Button("Recenter")) RecenterView();
        if (ImGui.IsItemHovered())
            ImGui.SetTooltip("Reset pan + zoom (centres on you, or the map centre in replay).");

        ImGui.SameLine();
        DrawCaptureScope();

        ImGui.SameLine();
        if (ImGui.Button("View")) ImGui.OpenPopup("##viewpop");
        DrawViewPopup();

        ImGui.SameLine();
        if (ImGui.Button("Filters")) ImGui.OpenPopup("##filterpop");
        DrawFilterPopup();

        if (_selectedId != 0)
        {
            ImGui.SameLine();
            ImGui.TextColored(ColId, $"sel 0x{_selectedId:X8}");
            ImGui.SameLine();
            if (ImGui.SmallButton("clear##sel")) _selectedId = 0;
        }
    }

    private void DrawCaptureScope()
    {
        ImGui.AlignTextToFramePadding();
        ImGui.TextDisabled("Track");
        ImGui.SameLine();
        ImGui.SetNextItemWidth(110f * ImGuiHelpers.GlobalScale);

        string[] labels = { "Always", "In combat", "In duty" };
        int cur = (int)Cfg.CaptureWhen;
        if (cur < 0 || cur >= labels.Length) cur = 0;
        if (ImGui.BeginCombo("##capwhen", labels[cur]))
        {
            for (int i = 0; i < labels.Length; i++)
                if (ImGui.Selectable(labels[i], i == cur))
                {
                    Cfg.CaptureWhen = (CaptureMode)i;
                    Cfg.Save();
                }
            ImGui.EndCombo();
        }
        if (ImGui.IsItemHovered())
            ImGui.SetTooltip(
                "When the map records actors and events:\n" +
                "Always – everywhere, even in town.\n" +
                "In combat – only while you're in a fight.\n" +
                "In duty – only inside instanced duties.");
    }

    private void DrawViewPopup()
    {
        if (!ImGui.BeginPopup("##viewpop")) return;
        CfgToggle("In-game map", () => Cfg.MapShowGameMap, v => Cfg.MapShowGameMap = v,
            "Use the zone's actual map art as the floor. Falls back to the grid where no map exists.");
        CfgToggle("Waymarks", () => Cfg.MapShowWaymarks, v => Cfg.MapShowWaymarks = v);
        CfgToggle("Names", () => Cfg.MapShowNames, v => Cfg.MapShowNames = v);
        CfgToggle("Hide dead", () => Cfg.MapHideDead, v => Cfg.MapHideDead = v,
            "Hide actors at 0 HP. The game keeps despawning actors in the table for a while after a pull.");
        CfgToggle("Job icons", () => Cfg.MapJobIcons, v => Cfg.MapJobIcons = v,
            "Show each player's job icon instead of a plain marker (live only).");

        ImGui.Separator();
        ImGui.TextDisabled("Sizes & shapes");

        float wm = Cfg.MapWaymarkSize;
        ImGui.SetNextItemWidth(160f * ImGuiHelpers.GlobalScale);
        if (ImGui.SliderFloat("Waymark size", ref wm, 6f, 48f, "%.0f")) { Cfg.MapWaymarkSize = wm; Cfg.Save(); }

        float ms = Cfg.MapMarkerScale;
        ImGui.SetNextItemWidth(160f * ImGuiHelpers.GlobalScale);
        if (ImGui.SliderFloat("Actor size", ref ms, 0.5f, 2.5f, "%.2fx")) { Cfg.MapMarkerScale = ms; Cfg.Save(); }

        ShapeCombo("Player shape", () => Cfg.MapPlayerShape, v => Cfg.MapPlayerShape = v);
        ShapeCombo("Enemy shape",  () => Cfg.MapEnemyShape,  v => Cfg.MapEnemyShape  = v);
        ImGui.EndPopup();
    }

    private void DrawFilterPopup()
    {
        if (!ImGui.BeginPopup("##filterpop")) return;
        ImGui.TextDisabled("Show on map");
        ImGui.Separator();
        CfgToggle("Players (you + party)", () => Cfg.MapShowPlayers, v => Cfg.MapShowPlayers = v);
        CfgToggle("Enemies / adds", () => Cfg.MapShowEnemies, v => Cfg.MapShowEnemies = v);
        CfgToggle("Allied NPCs", () => Cfg.MapShowAllies, v => Cfg.MapShowAllies = v,
            "Friendly battle NPCs (escort/duty allies).");
        CfgToggle("Pets / minions", () => Cfg.MapShowPets, v => Cfg.MapShowPets = v,
            "Carbuncle, fairy, chocobo, minions.");
        CfgToggle("Objects", () => Cfg.MapShowObjects, v => Cfg.MapShowObjects = v,
            "Exits, aetherytes, event objects, treasure, etc.");
        ImGui.Separator();
        CfgToggle("Hide unnamed clutter", () => Cfg.MapHideUnnamed, v => Cfg.MapHideUnnamed = v,
            "Drop nameless pets/objects (letterboxes, markers) from the map and list.");
        ImGui.EndPopup();
    }

    private static readonly string[] ShapeNames = { "Circle", "Triangle", "Square", "Diamond" };

    private static void ShapeCombo(string label, Func<int> get, Action<int> set)
    {
        int cur = Math.Clamp(get(), 0, ShapeNames.Length - 1);
        ImGui.SetNextItemWidth(160f * ImGuiHelpers.GlobalScale);
        if (ImGui.BeginCombo(label, ShapeNames[cur]))
        {
            for (int i = 0; i < ShapeNames.Length; i++)
                if (ImGui.Selectable(ShapeNames[i], i == cur)) { set(i); Cfg.Save(); }
            ImGui.EndCombo();
        }
    }

    private static void CfgToggle(string label, Func<bool> get, Action<bool> set, string? tip = null)
    {
        bool v = get();
        if (ImGui.Checkbox(label, ref v)) { set(v); Cfg.Save(); }
        if (tip != null && ImGui.IsItemHovered()) ImGui.SetTooltip(tip);
    }

    private static bool ModeButton(string label, bool active)
    {
        if (active) ImGui.PushStyleColor(ImGuiCol.Button, Ui.Accent);
        bool clicked = ImGui.Button(label);
        if (active) ImGui.PopStyleColor();
        return clicked;
    }

    // ---- shared coordinate math -------------------------------------------------

    private Vector2 ToScreen(float wx, float wz, Vector2 origin, float size)
    {
        float half  = size * 0.5f;
        float scale = half / _viewRadius;
        return new Vector2(
            origin.X + half + (wx - _centerX) * scale,
            origin.Y + half + (wz - _centerZ) * scale);
    }

    private (float wx, float wz) ToWorld(Vector2 sp, Vector2 origin, float size)
    {
        float half     = size * 0.5f;
        float perPixel = _viewRadius / half;
        return (
            _centerX + (sp.X - origin.X - half) * perPixel,
            _centerZ + (sp.Y - origin.Y - half) * perPixel);
    }

    private void RecenterView()
    {
        // Live: centre on the player. Replay: centre on the recorded zone's map.
        var me = _replayMode ? null : Plugin.ObjectTable.LocalPlayer;
        if (me != null)
        {
            _centerX = me.Position.X;
            _centerZ = me.Position.Z;
        }
        else
        {
            EnsureMapTexture(CurrentMapId());
            _centerX = -_mapOffX;
            _centerZ = -_mapOffZ;
        }
        _viewRadius = MathF.Min(40f, _maxRadius);
    }

    private void DrawCanvas(float size)
    {
        var origin = ImGui.GetCursorScreenPos();
        ImGui.InvisibleButton("##mapcanvas", new Vector2(size, size),
            ImGuiButtonFlags.MouseButtonLeft | ImGuiButtonFlags.MouseButtonRight);
        bool hovered = ImGui.IsItemHovered();
        bool active  = ImGui.IsItemActive();
        var  mouse   = ImGui.GetMousePos();

        // Wheel zooms toward the cursor, keeping the world point under it fixed.
        if (hovered)
        {
            float wheel = ImGui.GetIO().MouseWheel;
            if (wheel != 0f)
            {
                var (wx, wz) = ToWorld(mouse, origin, size);
                _viewRadius = Math.Clamp(_viewRadius - wheel * _viewRadius * 0.12f, 5f, _maxRadius);
                float half = size * 0.5f;
                float perPixel = _viewRadius / half;
                _centerX = wx - (mouse.X - origin.X - half) * perPixel;
                _centerZ = wz - (mouse.Y - origin.Y - half) * perPixel;
            }
        }

        // Left-drag pans; a left release without a drag counts as a click-select.
        if (active && ImGui.IsMouseDragging(ImGuiMouseButton.Left, 4f))
        {
            var d = ImGui.GetMouseDragDelta(ImGuiMouseButton.Left, 4f);
            float perPixel = _viewRadius / (size * 0.5f);
            _centerX -= d.X * perPixel;
            _centerZ -= d.Y * perPixel;
            ImGui.ResetMouseDragDelta(ImGuiMouseButton.Left);
            _dragging = true;
        }

        bool clicked = false;
        if (ImGui.IsMouseReleased(ImGuiMouseButton.Left))
        {
            clicked = hovered && !_dragging;
            _dragging = false;
        }

        var dl = ImGui.GetWindowDrawList();
        var a  = origin;
        var b  = new Vector2(origin.X + size, origin.Y + size);

        dl.PushClipRect(a, b, true);
        dl.AddRectFilled(a, b, ImGui.ColorConvertFloat4ToU32(new Vector4(0.10f, 0.10f, 0.11f, 1f)), 4f);
        // Grid stays as the base so there's always something to read; the game map
        // (where one exists) draws on top of it.
        DrawGrid(dl, origin, a, b, size);
        if (Cfg.MapShowGameMap) DrawGameMap(dl, origin, size);
        DrawCardinals(dl, a, b);
        // Live waymarks belong to the current zone; in replay they'd be wrong.
        if (Cfg.MapShowWaymarks && !_replayMode) DrawWaymarks(dl, origin, size);

        if (_replayMode) DrawReplayScene(dl, origin, size);
        else             DrawLiveScene(dl, origin, size, a, b, hovered, clicked, mouse);

        dl.PopClipRect();
        dl.AddRect(a, b, ImGui.ColorConvertFloat4ToU32(new Vector4(0.35f, 0.35f, 0.38f, 1f)), 4f);
    }

    // Draws the zone's actual in-game map texture as the floor, aligned to world
    // space via the Map sheet's size/offset so pan and zoom stay correct.
    private bool DrawGameMap(ImDrawListPtr dl, Vector2 origin, float size)
    {
        EnsureMapTexture(CurrentMapId());
        var wrap = _mapTex?.GetWrapOrDefault();
        if (wrap == null) return false;

        float worldHalf = 1024f / _mapScale;

        // Inside a house/apartment the game points the instance at the ward map,
        // but interior coordinates don't sit on it. If we're standing outside the
        // map's own bounds the art is meaningless, so fall back to the grid.
        if (!_replayMode)
        {
            var me = Plugin.ObjectTable.LocalPlayer;
            if (me != null)
            {
                float margin = worldHalf * 0.05f;
                if (MathF.Abs(me.Position.X + _mapOffX) > worldHalf + margin ||
                    MathF.Abs(me.Position.Z + _mapOffZ) > worldHalf + margin)
                    return false;
            }
        }

        var tl = ToScreen(-_mapOffX - worldHalf, -_mapOffZ - worldHalf, origin, size);
        var br = ToScreen(-_mapOffX + worldHalf, -_mapOffZ + worldHalf, origin, size);
        dl.AddImage(wrap.Handle, tl, br);
        return true;
    }

    // The map the game is actually showing right now. Residential districts swap
    // the active map per ward/subdivision, so the territory's default Map is wrong;
    // AgentMap tracks the live one. Replay uses the map recorded with the pull.
    private unsafe uint CurrentMapId()
    {
        if (_replayMode)
        {
            if (_replayMapId != 0) return _replayMapId;
            return TerritoryDefaultMap(_replayTerr != 0 ? _replayTerr : Plugin.ClientState.TerritoryType);
        }

        var am = AgentMap.Instance();
        if (am != null && am->CurrentMapId != 0) return am->CurrentMapId;
        return TerritoryDefaultMap(Plugin.ClientState.TerritoryType);
    }

    private static uint TerritoryDefaultMap(uint terr)
    {
        try
        {
            var tt = Plugin.DataManager.GetExcelSheet<LuminaTerritory>().GetRowOrDefault(terr);
            return tt?.Map.ValueNullable?.RowId ?? 0;
        }
        catch { return 0; }
    }

    private void EnsureMapTexture(uint mapId)
    {
        if (mapId == _mapId) return;
        _mapId     = mapId;
        _mapTex    = null;
        _mapScale  = 1f;
        _mapOffX   = 0f;
        _mapOffZ   = 0f;
        _maxRadius = 200f;

        try
        {
            if (mapId == 0) return;
            var map = Plugin.DataManager.GetExcelSheet<Lumina.Excel.Sheets.Map>().GetRowOrDefault(mapId);
            if (map is not { } m || m.RowId == 0) return;

            string id = m.Id.ExtractText();
            if (string.IsNullOrEmpty(id)) return;

            _mapScale = m.SizeFactor > 0 ? m.SizeFactor / 100f : 1f;
            _mapOffX  = m.OffsetX;
            _mapOffZ  = m.OffsetY;

            // Let the user zoom out far enough to frame the whole map of this zone.
            float worldHalf = 1024f / _mapScale;
            _maxRadius = Math.Clamp(worldHalf * 1.25f, 120f, 4000f);

            // Some instances point at a Map whose texture doesn't ship; GetFromGame
            // hands back a blank wrap regardless, so check the file exists first.
            string path = $"ui/map/{id}/{id.Replace("/", "")}_m.tex";
            if (Plugin.DataManager.FileExists(path))
                _mapTex = Plugin.TextureProvider.GetFromGame(path);
        }
        catch
        {
            _mapTex = null;
        }
    }

    private void DrawGrid(ImDrawListPtr dl, Vector2 origin, Vector2 a, Vector2 b, float size)
    {
        uint minor = ImGui.ColorConvertFloat4ToU32(new Vector4(0.22f, 0.22f, 0.24f, 0.45f));
        uint major = ImGui.ColorConvertFloat4ToU32(new Vector4(0.32f, 0.32f, 0.36f, 0.80f));

        float half  = size * 0.5f;
        float scale = half / _viewRadius;
        int   steps = (int)(_viewRadius / 5f) + 1;

        for (int k = -steps; k <= steps; k++)
        {
            float w = k * 5f;
            if (MathF.Abs(w) > _viewRadius + 0.01f) continue;
            uint col = k == 0 ? major : minor;
            float x = origin.X + half + w * scale;
            dl.AddLine(new Vector2(x, a.Y), new Vector2(x, b.Y), col);
            float y = origin.Y + half + w * scale;
            dl.AddLine(new Vector2(a.X, y), new Vector2(b.X, y), col);
        }
    }

    private static void DrawCardinals(ImDrawListPtr dl, Vector2 a, Vector2 b)
    {
        uint col = ImGui.ColorConvertFloat4ToU32(new Vector4(0.55f, 0.55f, 0.60f, 0.85f));
        float midX = (a.X + b.X) * 0.5f;
        float midY = (a.Y + b.Y) * 0.5f;
        dl.AddText(new Vector2(midX - 4f, a.Y + 3f),  col, "N");
        dl.AddText(new Vector2(midX - 4f, b.Y - 17f), col, "S");
        dl.AddText(new Vector2(b.X - 14f, midY - 7f), col, "E");
        dl.AddText(new Vector2(a.X + 5f,  midY - 7f), col, "W");
    }

    private unsafe void DrawWaymarks(ImDrawListPtr dl, Vector2 origin, float size)
    {
        var mc = MarkingController.Instance();
        if (mc == null) return;

        int i = 0;
        foreach (ref var marker in mc->FieldMarkers)
        {
            if (i >= WaymarkIcons.Length) break;
            if (marker.Active)
            {
                var sp = ToScreen(marker.X / 1000f, marker.Z / 1000f, origin, size);
                DrawIconAt(dl, sp, Math.Clamp(Cfg.MapWaymarkSize, 6f, 48f), WaymarkIcons[i]);
            }
            i++;
        }
    }

    // ---- live --------------------------------------------------------------------

    private void DrawLiveScene(
        ImDrawListPtr dl, Vector2 origin, float size, Vector2 a, Vector2 b,
        bool hovered, bool clicked, Vector2 mouse)
    {
        RebuildLiveActors();

        uint nearest    = 0;
        float nearestD2 = 14f * 14f;

        foreach (var act in _liveActors)
        {
            var sp = ToScreen(act.Pos.X, act.Pos.Z, origin, size);
            var col = CatColor(act.Cat);
            bool off = sp.X < a.X || sp.X > b.X || sp.Y < a.Y || sp.Y > b.Y;

            if (off)
            {
                var clamped = new Vector2(
                    Math.Clamp(sp.X, a.X + 7f, b.X - 7f),
                    Math.Clamp(sp.Y, a.Y + 7f, b.Y - 7f));
                var dim = col with { W = 0.85f };
                DrawDiamond(dl, clamped, 5f, ImGui.ColorConvertFloat4ToU32(dim), false);
                dl.AddText(new Vector2(clamped.X + 7f, clamped.Y - 6f),
                    ImGui.ColorConvertFloat4ToU32(dim),
                    $"{Shorten(act.Name)} ({act.Pos.X:0},{act.Pos.Z:0})");
                continue;
            }

            DrawFacing(dl, sp, act.Rot, col);
            DrawLiveMarker(dl, sp, act, col);

            if (act.Id == _selectedId)
                dl.AddCircle(sp, 10f, ImGui.ColorConvertFloat4ToU32(new Vector4(1f, 1f, 1f, 0.95f)), 16, 2f);
            else if (SearchHit(act))
                dl.AddCircle(sp, 9f, ImGui.ColorConvertFloat4ToU32(Ui.Gold), 16, 1.6f);

            if (act.Casting)
                dl.AddCircle(sp, 8f, ImGui.ColorConvertFloat4ToU32(new Vector4(1f, 0.55f, 0.30f, 0.9f)), 16, 1.5f);

            if (Cfg.MapShowNames && !string.IsNullOrEmpty(act.Name))
                dl.AddText(new Vector2(sp.X + 7f, sp.Y - 6f),
                    ImGui.ColorConvertFloat4ToU32(col with { W = 0.85f }), Shorten(act.Name));

            if (hovered)
            {
                float dx = mouse.X - sp.X, dy = mouse.Y - sp.Y;
                float d2 = dx * dx + dy * dy;
                if (d2 < nearestD2) { nearestD2 = d2; nearest = act.Id; }
            }
        }

        DrawLiveTethers(dl, origin, size);
        DrawLiveHeadmarkers(dl, origin, size);

        if (nearest != 0)
        {
            if (clicked)
            {
                _selectedId = nearest;
                ImGui.SetClipboardText($"{nearest:X8}");
            }
            if (ImGui.IsMouseClicked(ImGuiMouseButton.Right))
            {
                _ctxId = nearest;
                ImGui.OpenPopup("##actorctx");
            }
            if (!ImGui.IsPopupOpen("##actorctx"))
                ShowActorTooltip(nearest);
        }
        else if (clicked)
        {
            _selectedId = 0;
        }
    }

    private void RebuildLiveActors()
    {
        _liveActors.Clear();
        foreach (var o in Plugin.ObjectTable)
        {
            if (o == null) continue;
            try
            {
                var cat = Classify(o);
                if (!CategoryVisible(cat)) continue;

                string name = o.Name.TextValue;
                if (Cfg.MapHideUnnamed && string.IsNullOrEmpty(name)
                    && cat is MapCategory.Pet or MapCategory.Object) continue;

                float hp = -1f;
                bool casting = false;
                uint castId = 0;
                float castRemain = 0f;
                uint baseId = o.DataId;
                bool dead = false;
                uint job = 0;

                if (o is IBattleChara bc)
                {
                    baseId = bc.BaseId;
                    if (bc.MaxHp > 0)
                    {
                        hp = bc.CurrentHp / (float)bc.MaxHp * 100f;
                        dead = bc.CurrentHp == 0;
                    }
                    if (bc.IsCasting)
                    {
                        casting = true;
                        castId = bc.CastActionId;
                        castRemain = MathF.Max(0f, bc.TotalCastTime - bc.CurrentCastTime);
                    }
                }

                if (o is IPlayerCharacter pc)
                    job = pc.ClassJob.RowId;

                if (Cfg.MapHideDead && dead) continue;

                _liveActors.Add(new MapActor(
                    o.EntityId, name, cat, o.Position, o.Rotation,
                    o.DataId, baseId, hp, casting, castId, castRemain, job));
            }
            catch
            {
                // a single bad actor never breaks the whole frame
            }
        }
    }

    private static bool CategoryVisible(MapCategory cat) => cat switch
    {
        MapCategory.You or MapCategory.Party => Cfg.MapShowPlayers,
        MapCategory.Enemy                    => Cfg.MapShowEnemies,
        MapCategory.Ally                     => Cfg.MapShowAllies,
        MapCategory.Pet                      => Cfg.MapShowPets,
        _                                    => Cfg.MapShowObjects,
    };

    private void DrawLiveMarker(ImDrawListPtr dl, Vector2 sp, in MapActor act, Vector4 col)
    {
        float scale = Math.Clamp(Cfg.MapMarkerScale, 0.4f, 3f);
        bool  isPlayer = act.Cat is MapCategory.You or MapCategory.Party or MapCategory.Ally;

        if (isPlayer && Cfg.MapJobIcons && act.Job != 0)
        {
            float s    = (act.Cat == MapCategory.You ? 22f : 18f) * scale;
            float ring = s * 0.5f + 2f;
            dl.AddCircleFilled(sp, ring, ImGui.ColorConvertFloat4ToU32(new Vector4(0f, 0f, 0f, 0.55f)), 16);
            DrawIconAt(dl, sp, s, 62100 + act.Job);
            uint rc = act.Cat == MapCategory.You
                ? ImGui.ColorConvertFloat4ToU32(new Vector4(1f, 1f, 1f, 0.95f))
                : ImGui.ColorConvertFloat4ToU32(col with { W = 0.85f });
            dl.AddCircle(sp, ring, rc, 16, act.Cat == MapCategory.You ? 1.8f : 1.2f);
            return;
        }

        int   shape;
        float r;
        switch (act.Cat)
        {
            case MapCategory.You:
            case MapCategory.Party:
            case MapCategory.Ally:
                shape = Cfg.MapPlayerShape; r = act.Cat == MapCategory.You ? 8f : 6f; break;
            case MapCategory.Enemy:
                shape = Cfg.MapEnemyShape;  r = 7f; break;
            default:
                shape = 0; r = 5f; break;
        }
        DrawMarker(dl, sp, shape, r * scale, col, act.Cat == MapCategory.You);
    }

    private void DrawLiveTethers(ImDrawListPtr dl, Vector2 origin, float size)
    {
        uint col = ImGui.ColorConvertFloat4ToU32(new Vector4(0.90f, 0.45f, 1f, 0.70f));
        foreach (var t in _plugin.Capture.ActiveTethers)
        {
            var from = Plugin.ObjectTable.SearchById(t.From);
            var to   = Plugin.ObjectTable.SearchById(t.To);
            if (from == null || to == null) continue;
            dl.AddLine(
                ToScreen(from.Position.X, from.Position.Z, origin, size),
                ToScreen(to.Position.X,   to.Position.Z,   origin, size),
                col, 1.5f);
        }
    }

    private void DrawLiveHeadmarkers(ImDrawListPtr dl, Vector2 origin, float size)
    {
        foreach (var hm in _plugin.Capture.ActiveHeadmarkers)
        {
            var obj = Plugin.ObjectTable.SearchById(hm.ActorId);
            if (obj == null) continue;
            var sp = ToScreen(obj.Position.X, obj.Position.Z, origin, size);
            dl.AddCircle(sp, 7f, ImGui.ColorConvertFloat4ToU32(new Vector4(1f, 0.85f, 0.20f, 0.9f)), 12, 1.6f);
            DrawIconAt(dl, new Vector2(sp.X + 13f, sp.Y), 16f, hm.IconId);
        }
    }

    private void ShowActorTooltip(uint id)
    {
        var obj = Plugin.ObjectTable.SearchById(id);
        if (obj == null) return;

        ImGui.BeginTooltip();
        ImGui.TextColored(CatColor(Classify(obj)),
            string.IsNullOrEmpty(obj.Name.TextValue) ? "(unnamed)" : obj.Name.TextValue);
        ImGui.TextColored(ColId, $"Entity 0x{obj.EntityId:X8}");
        ImGui.TextDisabled($"Kind {obj.ObjectKind}   Data {obj.DataId}");

        if (obj is IBattleChara bc)
        {
            ImGui.TextDisabled($"BaseId {bc.BaseId} (0x{bc.BaseId:X})");
            if (bc.MaxHp > 0)
                ImGui.TextColored(ColParty, $"HP {bc.CurrentHp / (float)bc.MaxHp * 100f:0.#}%");
            if (bc.IsCasting)
            {
                float rem = MathF.Max(0f, bc.TotalCastTime - bc.CurrentCastTime);
                ImGui.TextColored(new Vector4(1f, 0.55f, 0.30f, 1f),
                    $"casting {ActionName(bc.CastActionId)} ({rem:0.0}s)");
            }
        }

        ImGui.TextDisabled($"pos ({obj.Position.X:0.0}, {obj.Position.Z:0.0})");
        ImGui.TextDisabled("right-click for actions");
        ImGui.EndTooltip();
    }

    private void DrawActorContextPopup()
    {
        if (!ImGui.BeginPopup("##actorctx")) return;
        ActorContextBody(_ctxId);
        ImGui.EndPopup();
    }

    // Authoring helpers: copy the identifiers/coords you reach for when writing a module.
    private void ActorContextBody(uint id)
    {
        var obj = Plugin.ObjectTable.SearchById(id);
        if (obj == null) { ImGui.TextDisabled("(no longer present)"); return; }

        string name = string.IsNullOrEmpty(obj.Name.TextValue) ? "(unnamed)" : obj.Name.TextValue;
        ImGui.TextColored(CatColor(Classify(obj)), name);
        ImGui.Separator();

        if (ImGui.MenuItem("Select / focus")) _selectedId = id;
        if (ImGui.MenuItem($"Copy entity id   0x{obj.EntityId:X8}"))
            ImGui.SetClipboardText($"0x{obj.EntityId:X8}");
        if (ImGui.MenuItem($"Copy data id     0x{obj.DataId:X}"))
            ImGui.SetClipboardText($"0x{obj.DataId:X}");

        if (obj is IBattleNpc bn && ImGui.MenuItem($"Copy name id     {bn.NameId}"))
            ImGui.SetClipboardText(bn.NameId.ToString());

        if (obj is IBattleChara bc)
        {
            if (ImGui.MenuItem($"Copy base id     0x{bc.BaseId:X}"))
                ImGui.SetClipboardText($"0x{bc.BaseId:X}");
            if (bc.IsCasting && ImGui.MenuItem($"Copy cast id     0x{bc.CastActionId:X} ({ActionName(bc.CastActionId)})"))
                ImGui.SetClipboardText($"0x{bc.CastActionId:X}");
        }

        ImGui.Separator();
        var p = obj.Position;
        if (ImGui.MenuItem($"Copy position    ({p.X:0.0}, {p.Z:0.0})"))
            ImGui.SetClipboardText($"new Vector3({p.X:0.###}f, {p.Y:0.###}f, {p.Z:0.###}f)");
        if (ImGui.MenuItem($"Copy heading     {obj.Rotation:0.000}"))
            ImGui.SetClipboardText($"{obj.Rotation:0.#####}f");
        if (ImGui.MenuItem("Copy name"))
            ImGui.SetClipboardText(obj.Name.TextValue);
    }

    // ---- side list (live) --------------------------------------------------------

    private void DrawActorListBody()
    {
        ImGui.SetNextItemWidth(-1);
        ImGui.InputTextWithHint("##mapsearch", "filter name / id (hex or dec)…", ref _search, 64);

        ImGui.TextDisabled($"{_liveActors.Count} actors");
        ImGui.Separator();

        DrawActorGroup("Enemies", MapCategory.Enemy);
        DrawActorGroup("You / Party", MapCategory.You, MapCategory.Party);
        DrawActorGroup("Allies / Pets", MapCategory.Ally, MapCategory.Pet);
        DrawActorGroup("Objects", MapCategory.Object);
    }

    private void DrawActorGroup(string label, params MapCategory[] cats)
    {
        bool any = false;
        foreach (var act in _liveActors)
        {
            if (Array.IndexOf(cats, act.Cat) < 0) continue;
            if (!SearchHit(act) && !string.IsNullOrEmpty(_search)) continue;
            if (!any)
            {
                ImGui.TextDisabled(label);
                any = true;
            }
            DrawActorRow(act);
        }
        if (any) ImGui.Spacing();
    }

    private void DrawActorRow(MapActor act)
    {
        bool selected = act.Id == _selectedId;
        string name = string.IsNullOrEmpty(act.Name) ? "(unnamed)" : act.Name;

        ImGui.PushStyleColor(ImGuiCol.Text, CatColor(act.Cat));
        if (ImGui.Selectable($"{name}##a{act.Id}", selected))
        {
            _selectedId = selected ? 0u : act.Id;
            if (_selectedId != 0) ImGui.SetClipboardText($"{act.Id:X8}");
        }
        ImGui.PopStyleColor();

        if (ImGui.BeginPopupContextItem($"##ctx{act.Id}"))
        {
            ActorContextBody(act.Id);
            ImGui.EndPopup();
        }

        if (ImGui.IsItemHovered())
            ImGui.SetTooltip($"0x{act.Id:X8}\nleft-click selects + copies · right-click for actions");

        ImGui.SameLine();
        ImGui.TextColored(ColId, $"0x{act.Id:X8}");
        if (act.HpPct >= 0f)
        {
            ImGui.SameLine();
            ImGui.TextDisabled($"{act.HpPct:0}%");
        }
        if (act.Casting)
        {
            ImGui.SameLine();
            ImGui.TextColored(new Vector4(1f, 0.55f, 0.30f, 1f), $"» {ActionName(act.CastId)}");
        }
    }

    private bool SearchHit(MapActor act)
    {
        if (string.IsNullOrEmpty(_search)) return false;
        if (act.Name.Contains(_search, StringComparison.OrdinalIgnoreCase)) return true;
        return IdMatches(act.Id, _search) || IdMatches(act.DataId, _search) || IdMatches(act.BaseId, _search);
    }

    // ---- replay ------------------------------------------------------------------

    private void EnsurePullCache()
    {
        if (_replayPull == 0)
        {
            var pulls = _plugin.Capture.Pulls;
            if (pulls.Count > 0) _replayPull = pulls[^1].Index;
        }

        if (_replayPull == _cachedPull) return;
        _cachedPull = _replayPull;
        _playT = 0;
        _playing = false;
        _pullEvents.Clear();
        _pullFrames.Clear();

        var pull = FindPull(_replayPull);
        if (pull == null)
        {
            _pullStart = DateTime.Now;
            _pullDuration = 1;
            return;
        }

        // Snap the map (and zoom-out range) to the exact map the pull was recorded on.
        if ((pull.Territory != 0 && pull.Territory != _replayTerr) || pull.MapId != _replayMapId)
        {
            _replayTerr  = pull.Territory;
            _replayMapId = pull.MapId;
            EnsureMapTexture(CurrentMapId());
            RecenterView();
        }

        _pullStart = pull.Start;
        foreach (var e in _plugin.Capture.Events)
            if (e.Pull == _replayPull) _pullEvents.Add(e);
        _pullEvents.Sort((x, y) => x.Seq.CompareTo(y.Seq));

        foreach (var f in _plugin.Capture.Frames)
            if (f.Pull == _replayPull) _pullFrames.Add(f);
        _pullFrames.Sort((x, y) => x.T.CompareTo(y.T));

        double framesEnd = _pullFrames.Count > 0 ? _pullFrames[^1].T : 0;
        double eventsEnd = _pullEvents.Count > 0 ? (_pullEvents[^1].Time - _pullStart).TotalSeconds : 0;
        var end = pull.End == DateTime.MinValue ? pull.Start : pull.End;
        double pullEnd = (end - _pullStart).TotalSeconds;
        _pullDuration = Math.Max(1.0, Math.Max(pullEnd, Math.Max(framesEnd, eventsEnd)));
    }

    private void JumpToLatestPull()
    {
        var pulls = _plugin.Capture.Pulls;
        if (pulls.Count == 0) return;
        _replayPull = pulls[^1].Index;
        _cachedPull = -1;
    }

    private CombatLogCapture.PullInfo? FindPull(int index)
    {
        foreach (var p in _plugin.Capture.Pulls)
            if (p.Index == index) return p;
        return null;
    }

    private void AdvancePlay(double dt)
    {
        if (!_playing) return;
        _playT += dt * _playSpeed;
        if (_playT >= _pullDuration)
        {
            _playT = _pullDuration;
            _playing = false;
        }
    }

    private void DrawReplayControls()
    {
        var pulls = _plugin.Capture.Pulls;
        if (pulls.Count == 0)
        {
            ImGui.TextDisabled("No pulls captured yet — start a fight (or set Track to Always).");
            return;
        }

        ImGui.AlignTextToFramePadding();
        ImGui.TextDisabled("Pull");
        ImGui.SameLine();
        ImGui.SetNextItemWidth(220f * ImGuiHelpers.GlobalScale);
        if (ImGui.BeginCombo("##pullsel", PullLabel(FindPull(_replayPull))))
        {
            for (int i = pulls.Count - 1; i >= 0; i--)
            {
                var p = pulls[i];
                if (ImGui.Selectable(PullLabel(p), p.Index == _replayPull))
                    _replayPull = p.Index;
            }
            ImGui.EndCombo();
        }

        ImGui.SameLine();
        if (ImGui.Button("Latest")) JumpToLatestPull();
        if (ImGui.IsItemHovered())
            ImGui.SetTooltip("Jump to the most recent pull (your last wipe / clear).");

        ImGui.SameLine();
        if (ImGui.Button(_playing ? "Pause" : "Play")) _playing = !_playing;
        ImGui.SameLine();
        if (ImGui.Button("Reset")) { _playT = 0; _playing = false; }

        ImGui.SameLine();
        ImGui.SetNextItemWidth(110f * ImGuiHelpers.GlobalScale);
        ImGui.SliderFloat("##speed", ref _playSpeed, 0.25f, 4f, "%.2fx");

        float t = (float)_playT;
        ImGui.SetNextItemWidth(-1);
        if (ImGui.SliderFloat("##timeline", ref t, 0f, (float)_pullDuration, FormatTime(t)))
        {
            _playT = t;
            _playing = false;
        }

        DrawNearReadout();
    }

    private void DrawNearReadout()
    {
        ImGui.TextDisabled($"t = {FormatTime((float)_playT)} / {FormatTime((float)_pullDuration)}");

        if (_pullFrames.Count == 0)
        {
            ImGui.SameLine();
            ImGui.TextColored(Ui.Gold, "· no movement recorded (only event markers)");
        }

        int shown = 0;
        for (int i = _pullEvents.Count - 1; i >= 0 && shown < 3; i--)
        {
            var e = _pullEvents[i];
            if (Offset(e) > _playT) continue;
            var (label, col) = KindTag(e.Kind);
            ImGui.SameLine();
            ImGui.TextColored(col, $"· {label} {Shorten(string.IsNullOrEmpty(e.Name) ? e.SourceName : e.Name)}");
            shown++;
        }
    }

    private CombatLogCapture.MapFrame? FrameAt(double t)
    {
        if (_pullFrames.Count == 0) return null;
        int lo = 0, hi = _pullFrames.Count - 1, best = 0;
        while (lo <= hi)
        {
            int mid = (lo + hi) / 2;
            if (_pullFrames[mid].T <= t) { best = mid; lo = mid + 1; }
            else hi = mid - 1;
        }
        return _pullFrames[best];
    }

    private void DrawReplayTrail(ImDrawListPtr dl, Vector2 origin, float size)
    {
        if (_selectedId == 0) return;
        const double window = 14.0;
        Vector2? prev = null;
        foreach (var f in _pullFrames)
        {
            if (f.T > _playT) break;
            if (_playT - f.T > window) continue;
            foreach (var s in f.Actors)
            {
                if (s.Id != _selectedId) continue;
                var sp = ToScreen(s.X, s.Z, origin, size);
                if (prev.HasValue)
                    dl.AddLine(prev.Value, sp,
                        ImGui.ColorConvertFloat4ToU32(new Vector4(1f, 1f, 1f, 0.45f)), 1.6f);
                prev = sp;
                break;
            }
        }
    }

    private void DrawReplayActors(ImDrawListPtr dl, Vector2 origin, float size)
    {
        var frame = FrameAt(_playT);
        if (frame == null) return;

        DrawReplayTrail(dl, origin, size);

        var a = origin;
        var b = new Vector2(origin.X + size, origin.Y + size);
        bool hovered = ImGui.IsItemHovered();
        var  mouse   = ImGui.GetMousePos();

        uint nearest = 0;
        float nearestD2 = 14f * 14f;
        CombatLogCapture.ActorSample nearestS = default;

        foreach (var s in frame.Actors)
        {
            var sp  = ToScreen(s.X, s.Z, origin, size);
            var col = KindColor(s.Kind);
            bool off = sp.X < a.X || sp.X > b.X || sp.Y < a.Y || sp.Y > b.Y;

            if (off)
            {
                var clamped = new Vector2(
                    Math.Clamp(sp.X, a.X + 7f, b.X - 7f),
                    Math.Clamp(sp.Y, a.Y + 7f, b.Y - 7f));
                DrawDiamond(dl, clamped, 5f, ImGui.ColorConvertFloat4ToU32(col with { W = 0.8f }), false);
                continue;
            }

            DrawFacing(dl, sp, s.Rot, col);
            {
                float scale = Math.Clamp(Cfg.MapMarkerScale, 0.4f, 3f);
                int shape; float r;
                switch (s.Kind)
                {
                    case ActorKind.You:   shape = Cfg.MapPlayerShape; r = 8f; break;
                    case ActorKind.Party: shape = Cfg.MapPlayerShape; r = 6f; break;
                    case ActorKind.Enemy: shape = Cfg.MapEnemyShape;  r = 7f; break;
                    default:              shape = 0;                  r = 5f; break;
                }
                DrawMarker(dl, sp, shape, r * scale, col, s.Kind == ActorKind.You);
            }

            if (s.Id == _selectedId)
                dl.AddCircle(sp, 10f, ImGui.ColorConvertFloat4ToU32(new Vector4(1f, 1f, 1f, 0.95f)), 16, 2f);
            if (s.CastId != 0)
                dl.AddCircle(sp, 8f, ImGui.ColorConvertFloat4ToU32(new Vector4(1f, 0.55f, 0.30f, 0.9f)), 16, 1.5f);

            if (Cfg.MapShowNames)
            {
                var nm = _plugin.Capture.FrameActorName(s.Id);
                if (!string.IsNullOrEmpty(nm))
                    dl.AddText(new Vector2(sp.X + 7f, sp.Y - 6f),
                        ImGui.ColorConvertFloat4ToU32(col with { W = 0.85f }), Shorten(nm));
            }

            if (hovered)
            {
                float dx = mouse.X - sp.X, dy = mouse.Y - sp.Y;
                float d2 = dx * dx + dy * dy;
                if (d2 < nearestD2) { nearestD2 = d2; nearest = s.Id; nearestS = s; }
            }
        }

        if (nearest != 0)
        {
            if (ImGui.IsMouseClicked(ImGuiMouseButton.Left))
            {
                _selectedId = nearest;
                ImGui.SetClipboardText($"{nearest:X8}");
            }
            ShowSampleTooltip(nearestS);
        }
    }

    private void ShowSampleTooltip(CombatLogCapture.ActorSample s)
    {
        ImGui.BeginTooltip();
        var nm = _plugin.Capture.FrameActorName(s.Id);
        ImGui.TextColored(KindColor(s.Kind), string.IsNullOrEmpty(nm) ? "(unknown)" : nm);
        ImGui.TextColored(ColId, $"Entity 0x{s.Id:X8}");
        if (s.HpPct >= 0f)
            ImGui.TextColored(ColParty, $"HP {s.HpPct:0.#}%");
        if (s.CastId != 0)
            ImGui.TextColored(new Vector4(1f, 0.55f, 0.30f, 1f), $"casting {ActionName(s.CastId)}");
        ImGui.TextDisabled($"pos ({s.X:0.0}, {s.Z:0.0})");
        ImGui.EndTooltip();
    }

    private void DrawReplayScene(ImDrawListPtr dl, Vector2 origin, float size)
    {
        DrawReplayActors(dl, origin, size);

        foreach (var e in _pullEvents)
        {
            double off = Offset(e);
            if (off > _playT) break;
            double age = _playT - off;

            switch (e.Kind)
            {
                case LogKind.CastStart:
                case LogKind.Ability:
                case LogKind.CastFinish:
                case LogKind.AbilityExtra:
                {
                    if (e.X == 0 && e.Y == 0) break;
                    if (age > ReplayFade) break;
                    float alpha = (float)Math.Clamp(1.0 - age / ReplayFade, 0.18, 1.0);
                    var col = new Vector4(1f, 0.55f, 0.30f, alpha);
                    var sp  = ToScreen(e.X, e.Y, origin, size);
                    dl.AddCircleFilled(sp, 4f, ImGui.ColorConvertFloat4ToU32(col));
                    DrawFacing(dl, sp, e.Heading, col);
                    if (age < 3.5)
                        dl.AddText(new Vector2(sp.X + 7f, sp.Y - 6f),
                            ImGui.ColorConvertFloat4ToU32(col with { W = alpha }), Shorten(e.Name));
                    break;
                }
                case LogKind.Added:
                {
                    if (e.X == 0 && e.Y == 0) break;
                    if (age > ReplayFade * 2) break;
                    float alpha = (float)Math.Clamp(1.0 - age / (ReplayFade * 2), 0.2, 0.9);
                    var sp = ToScreen(e.X, e.Y, origin, size);
                    var col = ColEnemy with { W = alpha };
                    DrawDiamond(dl, sp, 4f, ImGui.ColorConvertFloat4ToU32(col), false);
                    break;
                }
                case LogKind.Headmarker:
                {
                    if (age > ReplayFade) break;
                    var src = e.SourceId != 0 ? Plugin.ObjectTable.SearchById(e.SourceId) : null;
                    if (src == null) break;
                    var sp = ToScreen(src.Position.X, src.Position.Z, origin, size);
                    dl.AddCircle(sp, 7f, ImGui.ColorConvertFloat4ToU32(new Vector4(1f, 0.85f, 0.20f, 0.9f)), 12, 1.6f);
                    DrawIconAt(dl, new Vector2(sp.X + 13f, sp.Y), 16f, e.DataId);
                    break;
                }
            }
        }
    }

    private void DrawReplayListBody()
    {
        ImGui.TextDisabled($"{_pullEvents.Count} events · {_pullFrames.Count} frames · click to seek");
        if (_pullFrames.Count == 0)
            ImGui.TextColored(Ui.Gold, "no movement recorded for this pull");
        ImGui.Separator();

        if (ImGui.BeginTable("##rev", 2,
                ImGuiTableFlags.RowBg | ImGuiTableFlags.ScrollY))
        {
            ImGui.TableSetupColumn("t", ImGuiTableColumnFlags.WidthFixed, 50f * ImGuiHelpers.GlobalScale);
            ImGui.TableSetupColumn("event", ImGuiTableColumnFlags.WidthStretch);

            float rowH = ImGui.GetTextLineHeightWithSpacing();
            var clipper = new ImGuiListClipper();
            clipper.Begin(_pullEvents.Count, rowH);
            while (clipper.Step())
                for (int i = clipper.DisplayStart; i < clipper.DisplayEnd; i++)
                {
                    var e   = _pullEvents[i];
                    double off = Offset(e);
                    bool past = off <= _playT;

                    ImGui.TableNextRow();
                    ImGui.TableNextColumn();
                    var (label, col) = KindTag(e.Kind);
                    if (ImGui.Selectable($"{FormatTime((float)off)}##rev{e.Seq}", false,
                            ImGuiSelectableFlags.SpanAllColumns))
                    {
                        _playT = Math.Clamp(off, 0, _pullDuration);
                        _playing = false;
                    }

                    ImGui.TableNextColumn();
                    ImGui.TextColored(past ? col : col with { W = 0.4f }, label);
                    ImGui.SameLine();
                    var name = string.IsNullOrEmpty(e.Name) ? e.SourceName : e.Name;
                    if (past) ImGui.Text(name);
                    else      ImGui.TextDisabled(name);
                }
            clipper.End();
            ImGui.EndTable();
        }
    }

    private double Offset(LogEvent e) => (e.Time - _pullStart).TotalSeconds;

    // ---- drawing primitives ------------------------------------------------------

    private void DrawFacing(ImDrawListPtr dl, Vector2 sp, float heading, Vector4 col)
    {
        var dir = new Vector2(MathF.Sin(heading), MathF.Cos(heading));
        var tip = new Vector2(sp.X + dir.X * 16f, sp.Y + dir.Y * 16f);
        dl.AddLine(sp, tip, ImGui.ColorConvertFloat4ToU32(new Vector4(0f, 0f, 0f, MathF.Min(col.W, 0.8f))), 4f);
        dl.AddLine(sp, tip, ImGui.ColorConvertFloat4ToU32(col with { W = MathF.Min(col.W, 0.95f) }), 2f);
    }

    // Filled circle with a dark halo so actors stay readable over busy map art.
    private static void DrawDot(ImDrawListPtr dl, Vector2 c, float r, Vector4 col)
        => DrawMarker(dl, c, 0, r, col, false);

    // shape: 0 circle, 1 triangle, 2 square, 3 diamond. A dark halo + light rim
    // keeps every marker readable over busy map art.
    private static void DrawMarker(ImDrawListPtr dl, Vector2 c, int shape, float r, Vector4 col, bool emphasize)
    {
        float a   = col.W;
        uint  fill   = ImGui.ColorConvertFloat4ToU32(col);
        uint  halo   = ImGui.ColorConvertFloat4ToU32(new Vector4(0f, 0f, 0f, MathF.Min(a, 0.85f)));
        uint  rim    = ImGui.ColorConvertFloat4ToU32(new Vector4(1f, 1f, 1f, MathF.Min(a, emphasize ? 0.95f : 0.6f)));
        float rimW   = emphasize ? 1.8f : 1f;
        float o      = r + 1.6f;

        switch (shape)
        {
            case 1: // triangle (point up)
            {
                Tri(dl, c, o, halo, true, 0f);
                Tri(dl, c, r, fill, true, 0f);
                Tri(dl, c, r, rim, false, rimW);
                break;
            }
            case 2: // square
            {
                dl.AddRectFilled(new Vector2(c.X - o, c.Y - o), new Vector2(c.X + o, c.Y + o), halo, 1.5f);
                dl.AddRectFilled(new Vector2(c.X - r, c.Y - r), new Vector2(c.X + r, c.Y + r), fill, 1.5f);
                dl.AddRect(new Vector2(c.X - r, c.Y - r), new Vector2(c.X + r, c.Y + r), rim, 1.5f, ImDrawFlags.None, rimW);
                break;
            }
            case 3: // diamond
            {
                DrawDiamond(dl, c, r, fill, true);
                dl.AddLine(new Vector2(c.X, c.Y - r), new Vector2(c.X + r, c.Y), rim, rimW);
                dl.AddLine(new Vector2(c.X + r, c.Y), new Vector2(c.X, c.Y + r), rim, rimW);
                dl.AddLine(new Vector2(c.X, c.Y + r), new Vector2(c.X - r, c.Y), rim, rimW);
                dl.AddLine(new Vector2(c.X - r, c.Y), new Vector2(c.X, c.Y - r), rim, rimW);
                break;
            }
            default: // circle
            {
                dl.AddCircleFilled(c, o, halo, 20);
                dl.AddCircleFilled(c, r, fill, 20);
                dl.AddCircle(c, r, rim, 20, rimW);
                break;
            }
        }
    }

    private static void Tri(ImDrawListPtr dl, Vector2 c, float r, uint col, bool filled, float thick)
    {
        var p0 = new Vector2(c.X,            c.Y - r);
        var p1 = new Vector2(c.X + r * 0.92f, c.Y + r * 0.72f);
        var p2 = new Vector2(c.X - r * 0.92f, c.Y + r * 0.72f);
        if (filled) dl.AddTriangleFilled(p0, p1, p2, col);
        else        dl.AddTriangle(p0, p1, p2, col, thick);
    }

    private static void DrawDiamond(ImDrawListPtr dl, Vector2 c, float r, uint col, bool filled)
    {
        var top    = new Vector2(c.X, c.Y - r);
        var right  = new Vector2(c.X + r, c.Y);
        var bottom = new Vector2(c.X, c.Y + r);
        var left   = new Vector2(c.X - r, c.Y);
        if (filled)
        {
            uint shadow = ImGui.ColorConvertFloat4ToU32(new Vector4(0f, 0f, 0f, 0.85f));
            float o = r + 1.6f;
            dl.AddTriangleFilled(new Vector2(c.X, c.Y - o), new Vector2(c.X + o, c.Y), new Vector2(c.X, c.Y + o), shadow);
            dl.AddTriangleFilled(new Vector2(c.X, c.Y - o), new Vector2(c.X, c.Y + o), new Vector2(c.X - o, c.Y), shadow);
            dl.AddTriangleFilled(top, right, bottom, col);
            dl.AddTriangleFilled(top, bottom, left, col);
        }
        else
        {
            dl.AddLine(top, right, col, 1.5f);
            dl.AddLine(right, bottom, col, 1.5f);
            dl.AddLine(bottom, left, col, 1.5f);
            dl.AddLine(left, top, col, 1.5f);
        }
    }

    private void DrawIconAt(ImDrawListPtr dl, Vector2 center, float size, uint iconId)
    {
        if (iconId == 0) return;
        if (!_iconCache.TryGetValue(iconId, out var tex))
        {
            if (_iconCache.Count > 256) _iconCache.Clear();
            tex = Plugin.TextureProvider.GetFromGameIcon(new GameIconLookup(iconId));
            _iconCache[iconId] = tex;
        }
        var wrap = tex?.GetWrapOrDefault();
        if (wrap == null) return;
        float h = size * 0.5f;
        dl.AddImage(wrap.Handle,
            new Vector2(center.X - h, center.Y - h),
            new Vector2(center.X + h, center.Y + h));
    }

    // ---- helpers -----------------------------------------------------------------

    private static MapCategory Classify(IGameObject o)
    {
        if (o.EntityId == Plugin.PlayerState.EntityId) return MapCategory.You;

        switch (o.ObjectKind)
        {
            case ObjectKind.Pc:
                return MapCategory.Party;
            case ObjectKind.BattleNpc:
                if (o is IBattleNpc bn)
                {
                    byte k = (byte)bn.BattleNpcKind;
                    if (k is 2 or 3) return MapCategory.Pet;   // pet / chocobo
                    if (k == 9)      return MapCategory.Ally;  // allied npc
                }
                return MapCategory.Enemy;
            case ObjectKind.Companion:
                return MapCategory.Pet;                         // minions
            default:
                return MapCategory.Object;
        }
    }

    private static Vector4 CatColor(MapCategory cat) => cat switch
    {
        MapCategory.You    => ColYou,
        MapCategory.Party  => ColParty,
        MapCategory.Enemy  => ColEnemy,
        MapCategory.Ally   => ColAlly,
        MapCategory.Pet    => ColPet,
        _                  => ColObject,
    };

    private static Vector4 KindColor(ActorKind kind) => kind switch
    {
        ActorKind.You   => ColYou,
        ActorKind.Party => ColParty,
        ActorKind.Enemy => ColEnemy,
        _               => ColObject,
    };

    private static (string, Vector4) KindTag(LogKind kind) => kind switch
    {
        LogKind.CastStart    => ("startcast", new Vector4(1f, 0.55f, 0.30f, 1f)),
        LogKind.CastFinish   => ("endcast",   new Vector4(1f, 0.55f, 0.30f, 1f)),
        LogKind.Ability      => ("use",    new Vector4(0.95f, 0.80f, 0.45f, 1f)),
        LogKind.StatusGain   => ("gain",   new Vector4(0.55f, 0.85f, 1f, 1f)),
        LogKind.StatusLose   => ("lose",   new Vector4(0.55f, 0.55f, 0.60f, 1f)),
        LogKind.Death        => ("death",  ColEnemy),
        LogKind.Headmarker   => ("marker", new Vector4(0.85f, 0.55f, 1f, 1f)),
        LogKind.Tether       => ("tether", new Vector4(0.85f, 0.55f, 1f, 1f)),
        LogKind.Added        => ("add",    ColEnemy),
        LogKind.MapEffect    => ("mapfx",  new Vector4(0.45f, 0.90f, 0.80f, 1f)),
        _                    => (kind.ToString().ToLowerInvariant(), ColObject),
    };

    private static string ActionName(uint id)
    {
        if (id == 0) return "";
        var a = Plugin.DataManager.GetExcelSheet<LuminaAction>().GetRowOrDefault(id);
        var n = a?.Name.ExtractText();
        return string.IsNullOrEmpty(n) ? $"#{id}" : n;
    }

    private static string PullLabel(CombatLogCapture.PullInfo? p)
        => p == null ? "(none)" : $"{p.Label}  {p.Start:HH:mm:ss} · {p.Duration()} · {p.Events}";

    private static string FormatTime(float seconds)
    {
        if (seconds < 0) seconds = 0;
        int m = (int)(seconds / 60f);
        float s = seconds - m * 60f;
        return $"{m}:{s:00.0}";
    }

    private static string Shorten(string s)
        => string.IsNullOrEmpty(s) ? "" : (s.Length <= 18 ? s : s[..17] + "…");

    private static bool IdMatches(uint id, string search)
    {
        if (id == 0) return false;
        var t = search.Trim();
        if (t.StartsWith("0x", StringComparison.OrdinalIgnoreCase)) t = t[2..];
        if (t.Length == 0) return false;
        if (id.ToString("X").Contains(t, StringComparison.OrdinalIgnoreCase)) return true;
        if (id.ToString("X8").Contains(t, StringComparison.OrdinalIgnoreCase)) return true;
        return id.ToString().Contains(t, StringComparison.Ordinal);
    }

    private readonly struct MapActor
    {
        public readonly uint        Id;
        public readonly string      Name;
        public readonly MapCategory Cat;
        public readonly Vector3     Pos;
        public readonly float       Rot;
        public readonly uint        DataId;
        public readonly uint        BaseId;
        public readonly float       HpPct;
        public readonly bool        Casting;
        public readonly uint        CastId;
        public readonly float       CastRemain;
        public readonly uint        Job;

        public MapActor(uint id, string name, MapCategory cat, Vector3 pos, float rot,
            uint dataId, uint baseId, float hpPct, bool casting, uint castId, float castRemain, uint job)
        {
            Id = id; Name = name; Cat = cat; Pos = pos; Rot = rot;
            DataId = dataId; BaseId = baseId; HpPct = hpPct;
            Casting = casting; CastId = castId; CastRemain = castRemain; Job = job;
        }
    }
}
