using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Graphics.Environment;
using YapYapDraw.Engine;
using YapYapDraw.Engine.Module;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;
using YapYapDraw.Engine.Util;
using YapYapDraw.Engine.Vfx;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.Memory;
using YapYapDraw.Engine.Interop.ActionEffect;
using YapYapDraw.Logging;
using Lumina.Text.ReadOnly;
using LuminaCfc = Lumina.Excel.Sheets.ContentFinderCondition;
using LuminaAction = Lumina.Excel.Sheets.Action;

namespace YapYapDraw.Engine;

public sealed class FightModuleHost : IDisposable
{
    public sealed record MechView(string Key, string Display, uint Phase, bool HasConfig = false, Action? DrawConfig = null);

    private sealed class FightPack
    {
        public required BaseModule Host { get; init; }
        public required string Name { get; init; }
        public required string Display { get; init; }
        public required ModuleSetup.Category Category { get; init; }
        public required uint Cfc { get; init; }
        public required uint Territory { get; init; }
        public required List<ISpecialAction> Actions { get; init; }
        public required List<MechView> Mechanics { get; init; }
    }

    public sealed class FightView
    {
        public required string Key { get; init; }
        public required string Display { get; init; }
        public required ModuleSetup.Category Category { get; init; }
        public required uint Cfc { get; init; }
        public required uint Territory { get; init; }
        public required IReadOnlyList<MechView> Mechanics { get; init; }
        public required bool UseAutoDraw { get; init; }
        public required bool IsActive { get; init; }
    }

    public IReadOnlyList<FightView> Fights => _packs.Select(p => new FightView
    {
        Key         = p.Name,
        Display     = p.Display,
        Category    = p.Category,
        Cfc         = p.Cfc,
        Territory   = p.Territory,
        Mechanics   = p.Mechanics,
        UseAutoDraw = p.Host.UseAutoDraw,
        IsActive    = p.Territory != 0 && p.Territory == YapYapDraw.Plugin.ClientState.TerritoryType,
    }).ToList();

    private readonly IPluginLog _log;
    private readonly CombatLogCapture? _capture;
    private readonly List<FightPack> _packs = new();
    private readonly ResourceService? _resourceService;
    private FightPack? _active;
    private bool _hooksReady;
    private uint _lastTerritory;
    private uint _lastWeather;
    private bool _lastInCombat;
    private bool _lastForceUmad;
    private DateTime _allDeadSince = DateTime.MinValue;
    private bool _fallbackWiped;
    private static readonly TimeSpan FallbackWipeHold = TimeSpan.FromSeconds(1.5);

    private const string UmadFightKey = "DancingMad";

    private static bool ForceUmadActive => YapYapDraw.Plugin.ConfigStatic?.ForceUmadActive ?? false;

    public bool UmadForced => ForceUmadActive && _active?.Name == UmadFightKey;

    public bool HooksInstalled => _hooksReady;
    public int  ModuleCount    => _active?.Actions.Count ?? _packs.Sum(p => p.Actions.Count);
    public uint TerritoryId    => _active?.Territory ?? 0;
    public string FightName => _active == null
        ? "none"
        : UmadForced ? $"{_active.Name} (forced)" : _active.Name;

    public FightModuleHost(IPluginLog log, CombatLogCapture? capture = null)
    {
        _log = log;
        _capture = capture;

        foreach (var loaded in ModuleRegistry.LoadAll())
            RegisterFight(loaded.Host, loaded.Mechanics);

        _log.Information($"[YapYapDraw] loaded {_packs.Count} fight packs, {_packs.Sum(p => p.Actions.Count)} mechanics");

        try
        {
            ClientOmenHooks.Init();
            _hooksReady = true;
        }
        catch (Exception ex)
        {
            _hooksReady = false;
            _log.Error(ex, "[YapYapDraw] ClientOmenHooks.Init failed; omen drawing disabled");
        }

        try
        {
            _resourceService = new ResourceService();
            _resourceService.Init();
        }
        catch (Exception ex)
        {
            _resourceService = null;
            _log.Error(ex, "[YapYapDraw] ResourceService.Init failed; omen blocking disabled");
        }
    }

    private void RegisterFight(BaseModule meta, IEnumerable<ISpecialAction> actions)
    {
        var list = actions.ToList();
        var key = meta.GetType().Name;
        _packs.Add(new FightPack
        {
            Host      = meta,
            Name      = key,
            Display   = FightDisplayNames.For(key),
            Category  = meta.ModuleInfo.Category,
            Cfc       = meta.ModuleInfo.GroupID,
            Territory = ResolveTerritory(meta.ModuleInfo.GroupID),
            Actions   = list,
            Mechanics = list.Select(a => new MechView(a.Name ?? string.Empty, ResolveMechName(a), a.Phase, a.HasConfig, a.DrawConfig)).ToList(),
        });
    }

    private static bool HasCjk(string s)
    {
        foreach (var ch in s)
            if (ch >= 0x4E00 && ch <= 0x9FFF) return true;
        return false;
    }

    private static string ResolveMechName(ISpecialAction a)
    {
        var raw = a.Name ?? string.Empty;
        if (!HasCjk(raw)) return raw;

        foreach (var id in a.ActionID)
        {
            if (id == 0) continue;
            try
            {
                var row = YapYapDraw.Plugin.DataManager
                    .GetExcelSheet<LuminaAction>(Dalamud.Game.ClientLanguage.English)
                    .GetRowOrDefault(id);
                if (row is { } r)
                {
                    var name = r.Name.ExtractText();
                    if (!string.IsNullOrWhiteSpace(name)) return name;
                }
            }
            catch { }
        }

        return raw;
    }

    private static bool MasterOff => YapYapDraw.Plugin.ConfigStatic is { ModulesEnabled: false };

    private static bool FightDisabled(string key)
        => YapYapDraw.Plugin.ConfigStatic?.DisabledFights.Contains(key) ?? false;

    private static bool MechanicDisabled(string fightKey, string mech)
        => YapYapDraw.Plugin.ConfigStatic?.DisabledMechanics.Contains(fightKey + "/" + mech) ?? false;

    private uint ResolveTerritory(uint cfcId)
    {
        try
        {
            var row = YapYapDraw.Plugin.DataManager.GetExcelSheet<LuminaCfc>().GetRowOrDefault(cfcId);
            if (row is { } cfc)
            {
                var t = cfc.TerritoryType.RowId;
                if (t != 0) return t;
            }
        }
        catch (Exception ex) { _log.Warning($"[YapYapDraw] territory resolve failed: {ex.Message}"); }
        return 0;
    }

    private void ResolveActive()
    {
        var territory = YapYapDraw.Plugin.ClientState.TerritoryType;
        var byTerritory = _packs.FirstOrDefault(p => p.Territory != 0 && p.Territory == territory);
        if (byTerritory != null)
        {
            _active = byTerritory;
            return;
        }

        if (ForceUmadActive)
        {
            _active = _packs.FirstOrDefault(p => p.Name == UmadFightKey);
            return;
        }

        _active = null;
    }

    private bool InZone
    {
        get
        {
            if (_active == null) return false;
            if (UmadForced) return true;
            return _active.Territory == 0 || YapYapDraw.Plugin.ClientState.TerritoryType == _active.Territory;
        }
    }

    public void Tick()
    {
        uint weather = 0;
        try
        {
            unsafe
            {
                var env = EnvManager.Instance();
                if (env != null)
                    weather = env->ActiveWeather;
            }
        }
        catch { }

        var territory = YapYapDraw.Plugin.ClientState.TerritoryType;
        bool forceUmad = ForceUmadActive;
        if (territory != _lastTerritory)
        {
            _lastTerritory = territory;
            _lastWeather    = weather;
            _lastForceUmad = forceUmad;
            ResolveActive();
            ResetAll();
        }
        else if (forceUmad != _lastForceUmad)
        {
            _lastForceUmad = forceUmad;
            ResolveActive();
            if (forceUmad) ResetAll();
        }

        FightRuntime.SetWeather(weather);
        if (weather != _lastWeather)
        {
            uint old = _lastWeather;
            _lastWeather = weather;
            DispatchWeatherChange(old, weather);
        }

        bool inCombat = YapYapDraw.Plugin.Condition[Dalamud.Game.ClientState.Conditions.ConditionFlag.InCombat];
        if (inCombat && !_lastInCombat) ResetAll();
        if (!inCombat && _lastInCombat && !UmadForced) CleanVfx();
        _lastInCombat = inCombat;

        // DutyWiped clears drawings in a started duty (authoritative, handled via the
        // plugin's IDutyState subscription). Outside one (replays, open world) fall back
        // to a sustained all-dead read, held long enough that a mass raise won't trip it.
        if (!YapYapDraw.Plugin.DutyState.IsDutyStarted)
        {
            if (PartyAllDead())
            {
                if (_allDeadSince == DateTime.MinValue)
                    _allDeadSince = DateTime.Now;
                else if (!_fallbackWiped && DateTime.Now - _allDeadSince >= FallbackWipeHold)
                {
                    _fallbackWiped = true;
                    if (_active != null) WipeCleanup();
                }
            }
            else
            {
                _allDeadSince  = DateTime.MinValue;
                _fallbackWiped = false;
            }
        }
        else
        {
            _allDeadSince  = DateTime.MinValue;
            _fallbackWiped = false;
        }

        if (_active == null)
        {
            VfxBlocker.ClearSyncedBlocks();
        }
        else
        {
            bool fightOff = MasterOff || FightDisabled(_active.Name);

            if (fightOff)
                VfxBlocker.ClearSyncedBlocks();
            else
                VfxBlocker.SyncOmenBlocks(
                    new[] { _active.Host.BlockOmenMap },
                    new[] { _active.Host.BlockOmenPathMap },
                    FightRuntime.WeatherId,
                    _active.Host.ModuleInfo.GroupID);

            foreach (var a in _active.Actions)
            {
                try
                {
                    if (fightOff || MechanicDisabled(_active.Name, a.Name) || !WeatherOk(a))
                    {
                        foreach (var v in a.aoes)
                            if (v != null) v.Enable = false;
                        continue;
                    }

                    var active = a.ActiveAOEs.ToList();
                    var all = a.aoes.ToList();
                    if (active.Count > 0 && all.Count > 0)
                        foreach (var v in all)
                            if (v != null) v.Enable = active.Contains(v);
                    a.Update();
                }
                catch (Exception ex) { _log.Debug($"[YapYapDraw] module update: {ex.Message}"); }
            }
        }

        // Always drives every live StaticVfx (fight modules AND quick draws), so
        // user draws keep ticking even with no fight module loaded for the zone.
        try { FrameworkUpdateManager.Tick(); }
        catch (Exception ex) { _log.Debug($"[YapYapDraw] tick: {ex.Message}"); }
    }

    private static bool WeatherOk(ISpecialAction a)
        => a.WeatherID == 0 || a.WeatherID == FightRuntime.WeatherId;

    // The real wipe signal: clear every drawing and reset module state so the next
    // pull starts clean. Driven by IDutyState.DutyWiped/Recommenced/Completed.
    public void HandleDutyWipe() => WipeCleanup();

    private void WipeCleanup()
    {
        CleanVfx();
        ResetAll();
    }

    // True only when at least one player is present and none are alive. An empty
    // table (loading / fade-out) returns false so it can't read as a wipe.
    private static bool PartyAllDead()
    {
        bool sawPlayer = false;
        foreach (var p in Helper.PlayerHelper.AllPlayers)
        {
            if (p is not IBattleChara bc) continue;
            sawPlayer = true;
            if (!bc.IsDead && bc.CurrentHp > 0) return false;
        }
        return sawPlayer;
    }

    private void ResetAll()
    {
        VfxBlocker.ClearSyncedBlocks();
        _capture?.ResetLiveState();
        FightClientState.ClearEnmity();
        Data.Clear();
        foreach (var pack in _packs)
        {
            foreach (var a in pack.Actions)
            {
                try { a.Reset(); } catch (Exception ex) { _log.Debug($"[YapYapDraw] reset: {ex.Message}"); }
            }
        }
    }

    public void CleanVfx()
    {
        try { ClientOmenHooks.CleanAllVfx(); } catch (Exception ex) { _log.Debug($"[YapYapDraw] cleanvfx: {ex.Message}"); }
        foreach (var pack in _packs)
        {
            foreach (var a in pack.Actions)
            {
                try { a.Reset(); } catch (Exception ex) { _log.Debug($"[YapYapDraw] cleanvfx reset: {ex.Message}"); }
            }
        }
    }

    public void OnEvent(LogEvent e)
    {
        if (!_hooksReady) return;
        ResolveActive();
        if (!InZone || _active == null) return;
        if (MasterOff || FightDisabled(_active.Name)) return;

        try
        {
            switch (e.Kind)
            {
                case LogKind.CastStart:     HandleCast(e);       break;
                case LogKind.Ability:       HandleAbility(e);    break;
                case LogKind.StatusGain:    HandleStatus(e);     break;
                case LogKind.StatusLose:    HandleStatusRemove(e); break;
                case LogKind.MapEffect:     HandleMapEffect(e);  break;
                case LogKind.TimelineEvent: HandleTimeline(e);   break;
                case LogKind.TimelineSync:  HandleTimelineSync(e); break;
                case LogKind.Headmarker:    HandleHeadmarker(e); break;
                case LogKind.Added:         HandleAdded(e);      break;
                case LogKind.Tether:        HandleTether(e);     break;
                case LogKind.TetherCancel:  HandleTetherCancel(e); break;
                case LogKind.EventObject:     HandleEventObject(e); break;
                case LogKind.ActorControl:    HandleActorControl(e); break;
                case LogKind.ActorTargetVfx:  HandleActorTargetVfx(e); break;
            }
        }
        catch (Exception ex)
        {
            _log.Debug($"[YapYapDraw] dispatch {e.Kind}: {ex.Message}");
        }
    }

    public void HandleChatMessage(uint chatType, string content)
    {
        if (!_hooksReady) return;
        ResolveActive();
        if (!InZone || _active == null) return;
        if (MasterOff || FightDisabled(_active.Name)) return;

        foreach (var a in Actions)
        {
            try { a.OnChatMessage(chatType, content); }
            catch (Exception ex) { _log.Debug($"[YapYapDraw] chat dispatch: {ex.Message}"); }
        }
    }

    public void HandleNpcYell(ulong sourceId, ushort message)
    {
        if (!_hooksReady) return;
        ResolveActive();
        if (!InZone || _active == null) return;
        if (MasterOff || FightDisabled(_active.Name)) return;

        foreach (var a in Actions)
        {
            try { a.OnNpcYell(sourceId, message); }
            catch (Exception ex) { _log.Debug($"[YapYapDraw] npc-yell dispatch: {ex.Message}"); }
        }
    }

    private void DispatchWeatherChange(uint oldWeather, uint newWeather)
    {
        if (!_hooksReady) return;
        ResolveActive();
        if (!InZone || _active == null) return;
        if (MasterOff || FightDisabled(_active.Name)) return;

        foreach (var a in Actions)
        {
            try { a.OnWeatherChange(oldWeather, newWeather); }
            catch (Exception ex) { _log.Debug($"[YapYapDraw] weather dispatch: {ex.Message}"); }
        }
    }

    private IEnumerable<ISpecialAction> Actions
        => _active!.Actions
            .Where(a => !MechanicDisabled(_active.Name, a.Name))
            .Where(WeatherOk);

    private void HandleCast(LogEvent e)
    {
        var pos = Data.LastCastPositions.TryGetValue(e.SourceId, out var packetPos)
            ? packetPos
            : new Vector3(e.X, 0f, e.Y);

        var info = new ActorCastInfo
        {
            ActionId     = (ushort)e.DataId,
            DisplayDelay = (byte)e.Param1,
            CastTime     = e.Value,
            SourceId     = e.SourceId,
            TargetId     = e.TargetId,
            Facing       = new Angle(e.Heading),
            Pos          = pos,
        };
        foreach (var a in Actions)
        {
            if (a.ActionID.Contains(e.DataId))
                a.OnActionCast(info);
        }

        if (_active.Host.UseAutoDraw)
            AutoDrawModule.Run(info);
    }

    private void HandleAbility(LogEvent e)
    {
        var source = YapYapDraw.Plugin.ObjectTable.SearchById(e.SourceId);
        var target = e.TargetId != 0 ? YapYapDraw.Plugin.ObjectTable.SearchById(e.TargetId) : null;

        foreach (var sv in ClientOmenHooks.drawOmenElementList.ToArray())
            sv.OnHitEvent(e.DataId, target);
        foreach (var av in ClientOmenHooks.ActorVfxList.ToArray())
            av.OnHitEvent(e.DataId, target);

        var targetEffects = e.AbilityTargetIds.Length == 0
            ? Array.Empty<TargetEffect>()
            : e.AbilityTargetIds.Select(id => new TargetEffect { TargetID = id }).ToArray();

        var info = new ActorAbilityInfo
        {
            ActionId = e.DataId,
            Source   = source!,
            Target   = target,
            TargetEffects = targetEffects,
            Rotation = new Angle(e.Heading),
            Pos      = new Vector3(e.X, 0f, e.Y),
        };
        foreach (var a in Actions)
        {
            if (a.ActionID.Contains(e.DataId))
                a.OnAbilityCast(info);
        }
        foreach (var a in Actions)
            a.OnDrawQueue(info);
    }

    private void HandleStatus(LogEvent e)
    {
        var info = new ActorStatusChangeInfo
        {
            StatusID = e.DataId,
            Stack    = e.Count,
            TargetID = e.TargetId,
            SourceID = e.SourceId,
            Time     = e.Value,
        };
        foreach (var a in Actions)
            a.OnAddStatus(info);
    }

    private void HandleStatusRemove(LogEvent e)
    {
        var info = new ActorStatusChangeInfo
        {
            StatusID = e.DataId,
            Stack    = e.Count,
            TargetID = e.TargetId,
            SourceID = e.SourceId,
            Time     = e.Value,
        };
        foreach (var a in Actions)
            a.OnRemoveStatus(info);
    }

    private void HandleMapEffect(LogEvent e)
    {
        byte index = (byte)e.Param1;
        uint state = e.Category;
        ushort s1 = (ushort)(state & 0xFFFF);
        ushort s2 = (ushort)(state >> 16);
        foreach (var a in Actions)
        {
            a.OnEnvControl(index, state);
            a.OnMapEffect(index, s1, s2);
        }
    }

    private void HandleTimeline(LogEvent e)
    {
        var source = YapYapDraw.Plugin.ObjectTable.SearchById(e.SourceId);
        if (source == null) return;
        foreach (var a in Actions)
            a.OnActorPlayActionTimelineEvent(source, e.DataId);
    }

    private void HandleTimelineSync(LogEvent e)
    {
        var source = YapYapDraw.Plugin.ObjectTable.SearchById(e.TargetId);
        if (source == null) return;
        foreach (var a in Actions)
            a.OnActorPlayActionTimelineEvent(source, e.DataId);
    }

    private void HandleHeadmarker(LogEvent e)
    {
        var source = YapYapDraw.Plugin.ObjectTable.SearchById(e.SourceId);
        if (source == null) return;
        foreach (var a in Actions)
            a.OnTargetIconEvent(source, e.DataId, e.Param1);
    }

    private void HandleAdded(LogEvent e)
    {
        var obj = YapYapDraw.Plugin.ObjectTable.SearchById(e.SourceId);
        if (obj == null) return;
        foreach (var a in Actions)
            a.OnObjectCreatedEvent(obj);
    }

    private void HandleTether(LogEvent e)
    {
        foreach (var a in Actions)
            a.OnActorTetherEvent(e.SourceId, e.DataId, e.TargetId);
    }

    private void HandleTetherCancel(LogEvent e)
    {
        foreach (var a in Actions)
            a.OnActorTetherCancelEvent(e.SourceId);
    }

    private void HandleEventObject(LogEvent e)
    {
        ushort p1 = (ushort)e.Param1;
        ushort p2 = (ushort)e.Param2;
        foreach (var a in Actions)
            a.OnEventObjectAnimation(e.SourceId, p1, p2);
    }

    private void HandleActorControl(LogEvent e)
    {
        foreach (var a in Actions)
            a.OnActorControl(e.SourceId, e.Category, (uint)e.Param1, (uint)e.Param2, (uint)e.Param3, (uint)e.Param4);
    }

    private void HandleActorTargetVfx(LogEvent e)
    {
        foreach (var a in Actions)
            a.OnActorTargetVfx(e.SourceId, e.DataId);
    }

    public void Dispose()
    {
        try { ResetAll(); } catch { }
        try { VfxBlocker.Dispose(); } catch (Exception ex) { _log.Debug($"[YapYapDraw] vfx blocker dispose: {ex.Message}"); }
        try { _resourceService?.Dispose(); } catch (Exception ex) { _log.Debug($"[YapYapDraw] resource service dispose: {ex.Message}"); }
        try { ClientOmenHooks.DisposeHooks(); } catch (Exception ex) { _log.Debug($"[YapYapDraw] dispose: {ex.Message}"); }
    }
}
