using System;
using System.Collections.Generic;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Interface.Textures;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using LuminaAction = Lumina.Excel.Sheets.Action;
using LuminaTerritory = Lumina.Excel.Sheets.TerritoryType;

namespace YapYapDraw.Windows;

// A live top-down arena: real zone map art as the floor, scroll-zoom toward the
// cursor, right-drag to pan, and live actors (you, party, boss + casts). Used by
// the strat editor so authoring happens on the actual fight, not a blank grid.
public sealed class MapCanvas
{
    private static readonly Vector4 ColYou   = new(0.35f, 0.85f, 1f,    1f);
    private static readonly Vector4 ColParty = new(0.40f, 0.85f, 0.50f, 1f);
    private static readonly Vector4 ColEnemy = new(0.96f, 0.42f, 0.42f, 1f);
    private static readonly Vector4 ColAlly  = new(0.55f, 0.90f, 0.70f, 1f);

    private static readonly uint[] WaymarkIcons =
        { 61241, 61242, 61243, 61247, 61244, 61245, 61246, 61248 };

    private readonly Plugin _plugin;
    private readonly Dictionary<uint, ISharedImmediateTexture> _iconCache = new();

    public float ViewRadius = 30f;
    public float MaxRadius   = 200f;
    public float CenterX     = 100f;
    public float CenterZ     = 100f;
    public bool  ShowGameMap = false;
    public bool  ShowWaymarks = true;
    public bool  ShowNames   = true;
    public bool  JobIcons    = true;

    private uint  _mapId = uint.MaxValue;
    private ISharedImmediateTexture? _mapTex;
    private float _mapScale = 1f, _mapOffX, _mapOffZ;

    public MapCanvas(Plugin plugin) => _plugin = plugin;

    public readonly struct Frame
    {
        public Frame(Vector2 origin, float size, bool hovered, bool active, Vector2 mouse, ImDrawListPtr dl)
        { Origin = origin; Size = size; Hovered = hovered; Active = active; Mouse = mouse; Dl = dl; }
        public Vector2 Origin { get; }
        public float Size { get; }
        public bool Hovered { get; }
        public bool Active { get; }
        public Vector2 Mouse { get; }
        public ImDrawListPtr Dl { get; }
    }

    public Vector2 ToScreen(float wx, float wz, Vector2 origin, float size)
    {
        float half  = size * 0.5f;
        float scale = half / ViewRadius;
        return new Vector2(
            origin.X + half + (wx - CenterX) * scale,
            origin.Y + half + (wz - CenterZ) * scale);
    }

    public Vector2 ToWorld(Vector2 sp, Vector2 origin, float size)
    {
        float half     = size * 0.5f;
        float perPixel = ViewRadius / half;
        return new Vector2(
            CenterX + (sp.X - origin.X - half) * perPixel,
            CenterZ + (sp.Y - origin.Y - half) * perPixel);
    }

    public void RecenterOnPlayer()
    {
        var me = Plugin.ObjectTable.LocalPlayer;
        if (me != null) { CenterX = me.Position.X; CenterZ = me.Position.Z; }
        else { EnsureMapTexture(CurrentMapId()); CenterX = -_mapOffX; CenterZ = -_mapOffZ; }
        ViewRadius = MathF.Min(30f, MaxRadius);
    }

    // Right-drag pans (left stays free for placement). Wheel zooms toward cursor.
    public Frame Begin(string id, float size)
    {
        var origin = ImGui.GetCursorScreenPos();
        ImGui.InvisibleButton(id, new Vector2(size, size),
            ImGuiButtonFlags.MouseButtonLeft | ImGuiButtonFlags.MouseButtonRight | ImGuiButtonFlags.MouseButtonMiddle);
        bool hovered = ImGui.IsItemHovered();
        bool active  = ImGui.IsItemActive();
        var  mouse   = ImGui.GetMousePos();

        if (hovered)
        {
            float wheel = ImGui.GetIO().MouseWheel;
            if (wheel != 0f)
            {
                var w = ToWorld(mouse, origin, size);
                ViewRadius = Math.Clamp(ViewRadius - wheel * ViewRadius * 0.12f, 5f, MaxRadius);
                float half = size * 0.5f;
                float perPixel = ViewRadius / half;
                CenterX = w.X - (mouse.X - origin.X - half) * perPixel;
                CenterZ = w.Y - (mouse.Y - origin.Y - half) * perPixel;
            }
        }

        if (active && ImGui.IsMouseDragging(ImGuiMouseButton.Right, 2f))
        {
            var d = ImGui.GetMouseDragDelta(ImGuiMouseButton.Right, 2f);
            float perPixel = ViewRadius / (size * 0.5f);
            CenterX -= d.X * perPixel;
            CenterZ -= d.Y * perPixel;
            ImGui.ResetMouseDragDelta(ImGuiMouseButton.Right);
        }

        var dl = ImGui.GetWindowDrawList();
        var a  = origin;
        var b  = new Vector2(origin.X + size, origin.Y + size);

        dl.PushClipRect(a, b, true);
        dl.AddRectFilled(a, b, ImGui.ColorConvertFloat4ToU32(new Vector4(0.10f, 0.10f, 0.11f, 1f)), 4f);
        DrawGrid(dl, origin, a, b, size);
        if (ShowGameMap) DrawGameMap(dl, origin, size);
        DrawCardinals(dl, a, b);
        if (ShowWaymarks) DrawWaymarks(dl, origin, size);

        return new Frame(origin, size, hovered, active, mouse, dl);
    }

    // Drawn UNDER the actors (call before DrawLiveActors): a filled play-area floor
    // with a bright edge so the boundary between arena and the death wall is obvious
    // without relying on the zone map art.
    public void DrawArenaFloor(Frame f, byte shape, float radius, float cx, float cz)
    {
        if (radius < 0.5f) return;
        var dl = f.Dl;
        uint floor = ImGui.ColorConvertFloat4ToU32(new Vector4(0.62f, 0.50f, 0.34f, 0.22f));
        uint edge  = ImGui.ColorConvertFloat4ToU32(new Vector4(0.98f, 0.88f, 0.50f, 0.95f));
        uint wall  = ImGui.ColorConvertFloat4ToU32(new Vector4(0.85f, 0.20f, 0.20f, 0.55f));
        float perW = (f.Size * 0.5f) / ViewRadius;
        var c = ToScreen(cx, cz, f.Origin, f.Size);

        if (shape == 0)
        {
            float r = radius * perW;
            dl.AddCircleFilled(c, r, floor, 96);
            dl.AddCircle(c, r, wall, 96, 4f);
            dl.AddCircle(c, r, edge, 96, 2f);
        }
        else
        {
            var a = ToScreen(cx - radius, cz - radius, f.Origin, f.Size);
            var b = ToScreen(cx + radius, cz + radius, f.Origin, f.Size);
            dl.AddRectFilled(a, b, floor, 2f);
            dl.AddRect(a, b, wall, 2f, ImDrawFlags.None, 4f);
            dl.AddRect(a, b, edge, 2f, ImDrawFlags.None, 2f);
        }

        dl.AddLine(new Vector2(c.X - 5f, c.Y), new Vector2(c.X + 5f, c.Y), edge, 1f);
        dl.AddLine(new Vector2(c.X, c.Y - 5f), new Vector2(c.X, c.Y + 5f), edge, 1f);
    }

    public void End(Frame f)
    {
        var a = f.Origin;
        var b = new Vector2(f.Origin.X + f.Size, f.Origin.Y + f.Size);
        f.Dl.PopClipRect();
        f.Dl.AddRect(a, b, ImGui.ColorConvertFloat4ToU32(new Vector4(0.35f, 0.35f, 0.38f, 1f)), 4f);
    }

    public void DrawLiveActors(Frame f)
    {
        var origin = f.Origin;
        float size = f.Size;
        var dl = f.Dl;

        var me     = Plugin.ObjectTable.LocalPlayer;
        uint bossId = me?.TargetObject?.EntityId ?? 0;

        foreach (var o in Plugin.ObjectTable)
        {
            if (o == null) continue;
            MapCategory cat;
            try { cat = Classify(o); } catch { continue; }
            if (cat == MapCategory.Object || cat == MapCategory.Pet) continue;

            var sp  = ToScreen(o.Position.X, o.Position.Z, origin, size);
            var col = CatColor(cat);

            bool casting = false;
            uint castId  = 0;
            bool dead    = false;
            if (o is IBattleChara bc)
            {
                if (bc.MaxHp > 0) dead = bc.CurrentHp == 0;
                if (bc.IsCasting) { casting = true; castId = bc.CastActionId; }
            }
            if (dead && cat == MapCategory.Enemy) continue;

            bool isYou  = cat == MapCategory.You;
            bool isBoss = o.EntityId == bossId && cat == MapCategory.Enemy;

            if (cat == MapCategory.Enemy && o.HitboxRadius > 0.1f)
            {
                float perW = (size * 0.5f) / ViewRadius;
                float hr = o.HitboxRadius * perW;
                dl.AddCircleFilled(sp, hr, ImGui.ColorConvertFloat4ToU32(new Vector4(0.96f, 0.42f, 0.42f, isBoss ? 0.16f : 0.10f)), 40);
                dl.AddCircle(sp, hr, ImGui.ColorConvertFloat4ToU32(new Vector4(0.96f, 0.42f, 0.42f, isBoss ? 0.85f : 0.5f)), 40, isBoss ? 2f : 1.2f);
            }

            DrawFacing(dl, sp, o.Rotation, col);

            if ((cat is MapCategory.You or MapCategory.Party) && JobIcons && o is Dalamud.Game.ClientState.Objects.SubKinds.IPlayerCharacter pc && pc.ClassJob.RowId != 0)
            {
                float s = (isYou ? 22f : 18f);
                dl.AddCircleFilled(sp, s * 0.5f + 2f, ImGui.ColorConvertFloat4ToU32(new Vector4(0f, 0f, 0f, 0.55f)), 16);
                DrawIconAt(dl, sp, s, 62100 + pc.ClassJob.RowId);
                dl.AddCircle(sp, s * 0.5f + 2f, ImGui.ColorConvertFloat4ToU32(isYou ? new Vector4(1f, 1f, 1f, 0.95f) : col with { W = 0.85f }), 16, isYou ? 1.8f : 1.2f);
            }
            else
            {
                float r = isBoss ? 9f : (isYou ? 8f : 6f);
                DrawDot(dl, sp, r, col, isYou || isBoss);
            }

            if (isBoss)
                dl.AddCircle(sp, 13f, ImGui.ColorConvertFloat4ToU32(new Vector4(1f, 0.85f, 0.30f, 0.95f)), 20, 2f);

            if (casting)
            {
                dl.AddCircle(sp, 11f, ImGui.ColorConvertFloat4ToU32(new Vector4(1f, 0.55f, 0.30f, 0.95f)), 18, 1.8f);
                var nm = ActionName(castId);
                if (!string.IsNullOrEmpty(nm))
                    dl.AddText(new Vector2(sp.X + 9f, sp.Y + 6f),
                        ImGui.ColorConvertFloat4ToU32(new Vector4(1f, 0.65f, 0.40f, 1f)), nm);
            }

            if (ShowNames && !isYou && !string.IsNullOrEmpty(o.Name.TextValue))
                dl.AddText(new Vector2(sp.X + 8f, sp.Y - 7f),
                    ImGui.ColorConvertFloat4ToU32(col with { W = 0.85f }), Shorten(o.Name.TextValue));
        }

        foreach (var t in _plugin.Capture.ActiveTethers)
        {
            var from = Plugin.ObjectTable.SearchById(t.From);
            var to   = Plugin.ObjectTable.SearchById(t.To);
            if (from == null || to == null) continue;
            dl.AddLine(
                ToScreen(from.Position.X, from.Position.Z, origin, size),
                ToScreen(to.Position.X,   to.Position.Z,   origin, size),
                ImGui.ColorConvertFloat4ToU32(new Vector4(0.90f, 0.45f, 1f, 0.70f)), 1.5f);
        }

        foreach (var hm in _plugin.Capture.ActiveHeadmarkers)
        {
            var obj = Plugin.ObjectTable.SearchById(hm.ActorId);
            if (obj == null) continue;
            var sp = ToScreen(obj.Position.X, obj.Position.Z, origin, size);
            dl.AddCircle(sp, 8f, ImGui.ColorConvertFloat4ToU32(new Vector4(1f, 0.85f, 0.20f, 0.9f)), 12, 1.6f);
        }
    }

    // ---- floor -------------------------------------------------------------------

    private bool DrawGameMap(ImDrawListPtr dl, Vector2 origin, float size)
    {
        EnsureMapTexture(CurrentMapId());
        var wrap = _mapTex?.GetWrapOrDefault();
        if (wrap == null) return false;

        float worldHalf = 1024f / _mapScale;

        var me = Plugin.ObjectTable.LocalPlayer;
        if (me != null)
        {
            float margin = worldHalf * 0.05f;
            if (MathF.Abs(me.Position.X + _mapOffX) > worldHalf + margin ||
                MathF.Abs(me.Position.Z + _mapOffZ) > worldHalf + margin)
                return false;
        }

        var tl = ToScreen(-_mapOffX - worldHalf, -_mapOffZ - worldHalf, origin, size);
        var br = ToScreen(-_mapOffX + worldHalf, -_mapOffZ + worldHalf, origin, size);
        dl.AddImage(wrap.Handle, tl, br);
        return true;
    }

    private unsafe uint CurrentMapId()
    {
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
        _mapId    = mapId;
        _mapTex   = null;
        _mapScale = 1f;
        _mapOffX  = 0f;
        _mapOffZ  = 0f;
        MaxRadius = 200f;

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

            float worldHalf = 1024f / _mapScale;
            MaxRadius = Math.Clamp(worldHalf * 1.25f, 120f, 4000f);

            string path = $"ui/map/{id}/{id.Replace("/", "")}_m.tex";
            if (Plugin.DataManager.FileExists(path))
                _mapTex = Plugin.TextureProvider.GetFromGame(path);
        }
        catch { _mapTex = null; }
    }

    private void DrawGrid(ImDrawListPtr dl, Vector2 origin, Vector2 a, Vector2 b, float size)
    {
        uint minor = ImGui.ColorConvertFloat4ToU32(new Vector4(0.22f, 0.22f, 0.24f, 0.45f));
        uint major = ImGui.ColorConvertFloat4ToU32(new Vector4(0.32f, 0.32f, 0.36f, 0.80f));

        float half  = size * 0.5f;
        float scale = half / ViewRadius;
        int   steps = (int)(ViewRadius / 5f) + 1;

        for (int k = -steps; k <= steps; k++)
        {
            float w = k * 5f;
            if (MathF.Abs(w) > ViewRadius + 0.01f) continue;
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
                DrawIconAt(dl, sp, 22f, WaymarkIcons[i]);
            }
            i++;
        }
    }

    // ---- primitives --------------------------------------------------------------

    private static void DrawFacing(ImDrawListPtr dl, Vector2 sp, float heading, Vector4 col)
    {
        var dir = new Vector2(MathF.Sin(heading), MathF.Cos(heading));
        var tip = new Vector2(sp.X + dir.X * 16f, sp.Y + dir.Y * 16f);
        dl.AddLine(sp, tip, ImGui.ColorConvertFloat4ToU32(new Vector4(0f, 0f, 0f, MathF.Min(col.W, 0.8f))), 4f);
        dl.AddLine(sp, tip, ImGui.ColorConvertFloat4ToU32(col with { W = MathF.Min(col.W, 0.95f) }), 2f);
    }

    private static void DrawDot(ImDrawListPtr dl, Vector2 c, float r, Vector4 col, bool emphasize)
    {
        float a = col.W;
        uint fill = ImGui.ColorConvertFloat4ToU32(col);
        uint halo = ImGui.ColorConvertFloat4ToU32(new Vector4(0f, 0f, 0f, MathF.Min(a, 0.85f)));
        uint rim  = ImGui.ColorConvertFloat4ToU32(new Vector4(1f, 1f, 1f, MathF.Min(a, emphasize ? 0.95f : 0.6f)));
        dl.AddCircleFilled(c, r + 1.6f, halo, 20);
        dl.AddCircleFilled(c, r, fill, 20);
        dl.AddCircle(c, r, rim, 20, emphasize ? 1.8f : 1f);
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

    private enum MapCategory : byte { You, Party, Enemy, Ally, Pet, Object }

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
                    if (k is 2 or 3) return MapCategory.Pet;
                    if (k == 9)      return MapCategory.Ally;
                }
                return MapCategory.Enemy;
            case ObjectKind.Companion:
                return MapCategory.Pet;
            default:
                return MapCategory.Object;
        }
    }

    private static Vector4 CatColor(MapCategory cat) => cat switch
    {
        MapCategory.You   => ColYou,
        MapCategory.Party => ColParty,
        MapCategory.Enemy => ColEnemy,
        MapCategory.Ally  => ColAlly,
        _                 => new Vector4(0.66f, 0.66f, 0.70f, 1f),
    };

    private static string ActionName(uint id)
    {
        if (id == 0) return "";
        var a = Plugin.Actions.GetRowOrDefault(id);
        var n = a?.Name.ExtractText();
        return string.IsNullOrEmpty(n) ? $"#{id}" : n;
    }

    private static string Shorten(string s)
        => string.IsNullOrEmpty(s) ? "" : (s.Length <= 16 ? s : s[..15] + "\u2026");
}
