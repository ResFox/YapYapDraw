using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Configuration;
using YapYapDraw.QuickDraws;

namespace YapYapDraw;

public enum CaptureMode : byte
{
    Always,
    InCombat,
    InDuty,
}

[Serializable]
public sealed class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 2;

    public CaptureMode CaptureWhen { get; set; } = CaptureMode.Always;

    public float CustomAlpha { get; set; } = 1.5f;

    public int M2SSweetheartDrawMode { get; set; }
    public Vector4 TopP6CosmoArrowColor { get; set; } = new(1f, 1f, 0f, 1f);
    public Vector4 FruP5HellfireColor { get; set; } = new(1f, 1f, 1f, 2f);

    public bool ModulesEnabled { get; set; } = true;
    public HashSet<string> DisabledFights    { get; set; } = new();
    public HashSet<string> DisabledMechanics { get; set; } = new();

    // fightKey/mechanicName -> on/off; survives plugin rebuilds (authoritative over per-module JSON).
    public Dictionary<string, bool> ModuleEnabled { get; set; } = new();

    public Dictionary<string, string> ModuleConfigs { get; set; } = new();

    public const string QuickCategory = "Quick Draws";
    public bool QuickDrawsEnabled { get; set; } = true;
    public List<QuickDrawModule> QuickDrawModules { get; set; } = new();

    // The catch-all pack that draws made from a log line land in.
    public QuickDrawModule QuickModule()
    {
        var quick = QuickDrawModules.FirstOrDefault(m => !m.BuiltIn && m.Category == QuickCategory);
        if (quick != null) return quick;

        quick = new QuickDrawModule { Name = "My Quick Draws", Category = QuickCategory };
        QuickDrawModules.Insert(0, quick);
        return quick;
    }

    public bool ShowMapFx     { get; set; } = true;
    public bool ShowAdds      { get; set; } = true;
    public bool ShowControl   { get; set; } = true;
    public bool ShowPositions { get; set; } = true;

    // Actor-attached VFX (effect tells on players/bosses). High volume, so the
    // capture is opt-in; ShowVfx only filters the display.
    public bool LogGameVfx    { get; set; }
    public bool ShowVfx       { get; set; } = true;

    public bool ShowCasts    { get; set; } = true;
    public bool ShowStatus   { get; set; } = true;
    public bool ShowDeaths   { get; set; } = true;
    public bool ShowMarkers  { get; set; } = true;
    public bool ShowEnemies  { get; set; } = true;
    public bool ShowYou      { get; set; } = true;
    public bool ShowParty    { get; set; } = true;
    public bool ShowIds      { get; set; } = true;
    public bool ShowDecIds   { get; set; }

    // Live map view + filter state (persisted so it survives reloads).
    public bool MapShowGameMap  { get; set; } = true;
    public bool MapShowWaymarks { get; set; } = true;
    public bool MapShowNames    { get; set; }
    public bool MapHideDead     { get; set; }

    // Marker appearance. Shape: 0 circle, 1 triangle, 2 square, 3 diamond.
    public float MapWaymarkSize = 15f;
    public float MapMarkerScale = 1f;
    public int   MapPlayerShape = 1;
    public int   MapEnemyShape  = 0;
    public bool  MapJobIcons    = true;
    public bool MapShowPlayers  { get; set; } = true;
    public bool MapShowEnemies  { get; set; } = true;
    public bool MapShowAllies   { get; set; } = true;
    public bool MapShowPets     { get; set; }
    public bool MapShowObjects  { get; set; }
    public bool MapHideUnnamed  { get; set; } = true;

    public bool LogWindowOpen { get; set; } = true;
    public bool FirstRun    { get; set; } = true;
    public bool DebugHud    { get; set; }
    public bool OpenOnLogin { get; set; }

    // Dev: bind UMAD modules outside the duty (e.g. AnoMech sim in an Inn).
    public bool ForceUmadActive { get; set; }

    public void Save() => Plugin.PluginInterface.SavePluginConfig(this);
}
