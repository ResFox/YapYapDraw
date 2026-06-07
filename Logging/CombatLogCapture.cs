using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Hooking;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using YapYapDraw.Engine;
using YapYapDraw.Engine.Struct;
using YapYapDraw.Engine.Vfx;
using FFXIVClientStructs.FFXIV.Common.Math;
using ActionEffectHandler = FFXIVClientStructs.FFXIV.Client.Game.Character.ActionEffectHandler;
using LuminaAction = Lumina.Excel.Sheets.Action;
using LuminaStatus = Lumina.Excel.Sheets.Status;

namespace YapYapDraw.Logging;

public sealed class CombatLogCapture : IDisposable
{
    private readonly Configuration _config;
    private readonly IPluginLog    _log;

    private readonly Dictionary<uint, ActorState> _actors = new();
    private readonly HashSet<uint> _eventObjs       = new();
    private readonly HashSet<uint> _eventObjScratch = new();
    private readonly HashSet<(uint id, uint src)> _statusScratch = new();
    private readonly List<LogEvent>               _events = new();
    private long _seq;

    // Events captured inside the native game hooks are parked here and dispatched
    // on the next framework tick. The trigger/script host runs from the tick, never
    // from inside a hook, so it can't re-enter or stall the game's hook chain.
    private readonly Queue<(LogEvent e, bool addToLog)> _hookQueue = new();
    private readonly object _hookQueueLock = new();

    public long      TotalEmitted { get; private set; }
    public DateTime  LastEventAt  { get; private set; } = DateTime.MinValue;
    private readonly long[] _kindCounts = new long[20];
    private readonly List<(uint From, uint To, ushort Id)> _activeTethers = new();
    private readonly Dictionary<uint, uint> _activeHeadmarkers = new();

    public readonly record struct LiveTether(uint From, uint To, ushort Id);
    public readonly record struct LiveHeadmarker(uint ActorId, uint IconId);

    public IReadOnlyList<LiveTether> ActiveTethers
    {
        get
        {
            var list = new List<LiveTether>(_activeTethers.Count);
            foreach (var t in _activeTethers)
                list.Add(new LiveTether(t.From, t.To, t.Id));
            return list;
        }
    }

    public IReadOnlyList<LiveHeadmarker> ActiveHeadmarkers
    {
        get
        {
            var list = new List<LiveHeadmarker>(_activeHeadmarkers.Count);
            foreach (var kv in _activeHeadmarkers)
                list.Add(new LiveHeadmarker(kv.Key, kv.Value));
            return list;
        }
    }

    public long KindCount(LogKind k) => _kindCounts[(int)k];
    public int ActorsTracked => _actors.Count;

    private readonly List<PullInfo> _pulls = new();
    private int      _currentPull;
    private bool     _inCombat;
    private DateTime _combatLeftAt = DateTime.MinValue;

    public IReadOnlyList<PullInfo> Pulls => _pulls;

    // Position recorder: a coarse per-tick snapshot of every actor so a pull can be
    // scrubbed afterwards. Names are interned so frames stay cheap.
    private const double SnapshotInterval = 1.0 / 8.0;
    private const int    MaxFrames        = 60000;
    private const int    MaxPersistFrames = 24000;
    private readonly List<MapFrame>          _frames     = new();
    private readonly Dictionary<uint, string> _frameNames = new();
    private readonly List<ActorSample>        _frameScratch = new();
    private DateTime _lastSnapshot = DateTime.MinValue;

    public IReadOnlyList<MapFrame> Frames => _frames;
    public string FrameActorName(uint id) => _frameNames.TryGetValue(id, out var n) ? n : "";

    public readonly struct ActorSample
    {
        public readonly uint      Id;
        public readonly ActorKind Kind;
        public readonly float     X;
        public readonly float     Z;
        public readonly float     Rot;
        public readonly float     HpPct;
        public readonly uint      CastId;

        public ActorSample(uint id, ActorKind kind, float x, float z, float rot, float hpPct, uint castId)
        {
            Id = id; Kind = kind; X = x; Z = z; Rot = rot; HpPct = hpPct; CastId = castId;
        }
    }

    public sealed class MapFrame
    {
        public int           Pull;
        public double        T;
        public ActorSample[] Actors = Array.Empty<ActorSample>();
    }

    public event Action<LogEvent>? OnEvent;

    public bool   ActionEffectInstalled { get; private set; }
    public bool   CastHookInstalled     { get; private set; }
    public string InstallError { get; private set; } = "";

    private unsafe delegate void ActorCastDelegate(uint casterId, ActorCast* data);
    private Hook<ActorCastDelegate>? _castHook;
    private const string CastSig = "40 53 57 48 81 EC ?? ?? ?? ?? 48 8B FA 8B D1";

    private unsafe delegate void ProcessActionEffectDelegate(
        uint casterEntityId, Character* casterPtr, Vector3* targetPos,
        ActionEffectHandler.Header* header, ActionEffectHandler.TargetEffects* effects,
        GameObjectId* targetEntityIds);

    private Hook<ProcessActionEffectDelegate>? _actionEffectHook;

    // ActorControl carries headmarkers (category 34 = TargetIcon) and tethers
    // (category 35). Reading them here means they work from memory in replays,
    // not just from an external log feed.
    // Must match FFXIVClientStructs' HandleActorControlPacket exactly
    // (10x uint, then a GameObjectId and a bool), or Dalamud's hook verifier
    // flags it as crash-prone.
    private delegate void ProcessActorControlDelegate(
        uint actorId, uint category, uint p1, uint p2, uint p3, uint p4,
        uint p5, uint p6, uint p7, uint p8, GameObjectId targetId, bool replaying);

    private Hook<ProcessActorControlDelegate>? _actorControlHook;

    // MapEffect packets drive arena-state callouts (snaking platforms, arena
    // splits, etc.). The game calls this per incoming map-effect update with the
    // tile index and the two flag words; flags = s1 | (s2 << 16).
    private unsafe delegate void ProcessMapEffectDelegate(void* self, uint index, ushort s1, ushort s2);
    private Hook<ProcessMapEffectDelegate>? _mapEffectHook;
    private const string MapEffectSig =
        "48 89 5C 24 ?? 48 89 6C 24 ?? 48 89 74 24 ?? 57 48 83 EC 20 8B FA 41 0F B7 E8";
    public bool   MapEffectInstalled { get; private set; }
    public string MapEffectError     { get; private set; } = "";

    // Play-action-timeline SYNC: the server packet that maps a parent actor to the
    // child actors it splits into (e.g. M12S clone splits, timeline id 0x11D3).
    // First entry is the owner; the list terminates at 0xE0000000.
    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, Pack = 1)]
    private unsafe struct PlayActionTimelineSyncPacket
    {
        public fixed uint   EntityIds[10];
        public fixed ushort TimelineIds[10];
    }
    private unsafe delegate void ProcessPlayActionTimelineSyncDelegate(PlayActionTimelineSyncPacket* data);
    private Hook<ProcessPlayActionTimelineSyncDelegate>? _timelineSyncHook;
    private const string TimelineSyncSig =
        "48 8B D1 48 8D 0D ?? ?? ?? ?? E9 ?? ?? ?? ?? CC CC CC CC CC CC CC CC CC CC CC CC CC CC CC CC CC 40 53 56";
    public bool   TimelineSyncInstalled { get; private set; }

    // Actor-attached VFX create (the game's effect "tells" played on a caster/target).
    // Same function the draw engine itself calls, so a pass-through is required.
    private delegate nint ActorVfxCreateDelegate(nint path, nint caster, nint target, float a4, char a5, ushort a6, char a7);
    private Hook<ActorVfxCreateDelegate>? _actorVfxHook;
    private const string ActorVfxSig =
        "40 53 55 56 57 48 81 EC ?? ?? ?? ?? 0F 29 B4 24 ?? ?? ?? ?? 48 8B 05 ?? ?? ?? ?? 48 33 C4 48 89 84 24 ?? ?? ?? ?? 0F B6 AC 24 ?? ?? ?? ?? 0F 28 F3 49 8B F8";
    public bool   ActorVfxInstalled { get; private set; }
    public string ActorVfxError     { get; private set; } = "";
    public string TimelineSyncError     { get; private set; } = "";
    // Most recent distinct "FLAGS@LOCATION" map-effect values, for diagnosing why
    // arena-state triggers (snaking tiles) match or miss. Newest last.
    private readonly List<string> _recentMapFx = new();
    public string RecentMapEffects { get; private set; } = "";

    private const uint CategoryTargetIcon = 34;
    private const uint CategoryPlayActionTimeline = 407;
    private const uint CategoryEventObjectAnim    = 413;

    public unsafe CombatLogCapture(Configuration config, IGameInteropProvider interop, IPluginLog log)
    {
        _config = config;
        _log    = log;

        try
        {
            _actionEffectHook = interop.HookFromSignature<ProcessActionEffectDelegate>(
                ActionEffectHandler.Addresses.Receive.String, ActionEffectDetour);
            _actionEffectHook.Enable();
            ActionEffectInstalled = true;
        }
        catch (Exception ex)
        {
            InstallError = ex.Message;
            _log.Error(ex, "[YapYapDraw] failed to install ActionEffect hook");
        }

        try
        {
            _castHook = interop.HookFromSignature<ActorCastDelegate>(CastSig, ActorCastDetour);
            _castHook.Enable();
            CastHookInstalled = true;
        }
        catch (Exception ex)
        {
            _log.Error(ex, "[YapYapDraw] failed to install ActorCast hook");
        }

        try
        {
            _actorControlHook = interop.HookFromSignature<ProcessActorControlDelegate>(
                "E8 ?? ?? ?? ?? 0F B7 0B 83 E9 64", ActorControlDetour);
            _actorControlHook.Enable();
            ActorControlInstalled = true;
        }
        catch (Exception ex)
        {
            _log.Error(ex, "[YapYapDraw] failed to install ActorControl hook");
        }

        try
        {
            _mapEffectHook = interop.HookFromSignature<ProcessMapEffectDelegate>(
                MapEffectSig, ProcessMapEffectDetour);
            _mapEffectHook.Enable();
            MapEffectInstalled = true;
        }
        catch (Exception ex)
        {
            // Optional feed: a few arena-state callouts won't fire, everything
            // else is unaffected. Logged quietly so it isn't mistaken for a fault.
            MapEffectError = ex.Message;
            _log.Information($"[YapYapDraw] MapEffect feed unavailable on this game build: {ex.Message}");
        }

        try
        {
            _timelineSyncHook = interop.HookFromSignature<ProcessPlayActionTimelineSyncDelegate>(
                TimelineSyncSig, ProcessTimelineSyncDetour);
            _timelineSyncHook.Enable();
            TimelineSyncInstalled = true;
        }
        catch (Exception ex)
        {
            // Optional feed: only the clone-split safe spots depend on
            // it; the rest of capture is unaffected.
            TimelineSyncError = ex.Message;
            _log.Information($"[YapYapDraw] TimelineSync feed unavailable on this game build: {ex.Message}");
        }

        VfxContainerHooks.Init(this, interop, Plugin.SigScanner, _log);

        try
        {
            nint addr = Plugin.SigScanner.ScanText(ActorVfxSig);
            _actorVfxHook = interop.HookFromAddress<ActorVfxCreateDelegate>(addr, ActorVfxDetour);
            _actorVfxHook.Enable();
            ActorVfxInstalled = true;
        }
        catch (Exception ex)
        {
            ActorVfxError = ex.Message;
            _log.Information($"[YapYapDraw] VFX feed unavailable on this game build: {ex.Message}");
        }

        LoadFromDisk();
    }

    private nint ActorVfxDetour(nint path, nint caster, nint target, float a4, char a5, ushort a6, char a7)
    {
        var ret = _actorVfxHook!.Original(path, caster, target, a4, a5, a6, a7);
        try
        {
            if (_config.LogGameVfx && ShouldCapture() && path != 0)
            {
                string p = System.Runtime.InteropServices.Marshal.PtrToStringUTF8(path) ?? "";
                if (p.Length > 0)
                {
                    var srcObj = AddressToObject(caster);
                    var tgtObj = AddressToObject(target);
                    QueueFromHook(new LogEvent
                    {
                        Kind       = LogKind.Vfx,
                        Name       = p,
                        SourceId   = srcObj?.EntityId ?? 0,
                        SourceName = srcObj?.Name.TextValue ?? "",
                        SourceKind = srcObj is IBattleChara sbc ? Classify(sbc) : ActorKind.Other,
                        TargetId   = tgtObj?.EntityId ?? 0,
                        TargetName = tgtObj?.Name.TextValue ?? "",
                        TargetKind = tgtObj is IBattleChara tbc ? Classify(tbc) : ActorKind.Other,
                    }, addToLog: _config.ShowVfx);
                }
            }
        }
        catch (Exception ex) { _log.Debug($"[YapYapDraw] vfx detour: {ex.Message}"); }
        return ret;
    }

    private static IGameObject? AddressToObject(nint addr)
    {
        if (addr == 0) return null;
        foreach (var o in Plugin.ObjectTable)
            if (o.Address == addr) return o;
        return null;
    }

    private unsafe void ProcessTimelineSyncDetour(PlayActionTimelineSyncPacket* data)
    {
        _timelineSyncHook!.Original(data);
        try
        {
            if (!ShouldCapture() || data == null) return;
            uint owner = 0;
            for (var i = 0; i < 10; i++)
            {
                uint id = data->EntityIds[i];
                if (id == 0xE0000000) break;
                if (owner == 0) owner = id;
                var sObj = Plugin.ObjectTable.SearchById(owner);
                QueueFromHook(new LogEvent
                {
                    Kind       = LogKind.TimelineSync,
                    SourceId   = owner,
                    SourceName = sObj?.Name.TextValue ?? "",
                    SourceKind = sObj is IBattleChara sbc ? Classify(sbc) : ActorKind.Other,
                    TargetId   = id,
                    DataId     = data->TimelineIds[i],
                    Name       = $"TimelineSync {data->TimelineIds[i]:X4}",
                }, _config.ShowControl);
            }
        }
        catch (Exception ex)
        {
            _log.Debug($"[YapYapDraw] timeline-sync error: {ex.Message}");
        }
    }

    public bool ActorControlInstalled { get; private set; }

    public void Dispose()
    {
        SaveToDisk();
        try { _actionEffectHook?.Dispose(); } catch { }
        try { _castHook?.Dispose(); } catch { }
        try { _actorControlHook?.Dispose(); } catch { }
        try { _mapEffectHook?.Dispose(); } catch { }
        try { _timelineSyncHook?.Dispose(); } catch { }
        try { _actorVfxHook?.Dispose(); } catch { }
        VfxContainerHooks.Dispose();
    }

    private unsafe void ProcessMapEffectDetour(void* self, uint index, ushort s1, ushort s2)
    {
        _mapEffectHook!.Original(self, index, s1, s2);
        try
        {
            if (!ShouldCapture()) return;
            uint flags = s1 | ((uint)s2 << 16);
            var tag = $"{flags:X8}@{index:X2}";
            if (_recentMapFx.Count == 0 || _recentMapFx[^1] != tag)
            {
                _recentMapFx.Add(tag);
                if (_recentMapFx.Count > 8) _recentMapFx.RemoveAt(0);
                RecentMapEffects = string.Join(" ", _recentMapFx);
            }
            QueueFromHook(new LogEvent
            {
                Kind       = LogKind.MapEffect,
                SourceKind = ActorKind.Other,
                Name       = "MapEffect",
                Category   = flags,
                Param1     = index,
            }, _config.ShowMapFx);
        }
        catch (Exception ex)
        {
            _log.Debug($"[YapYapDraw] map-effect error: {ex.Message}");
        }
    }

    public IReadOnlyList<LogEvent> Events => _events;

    public void Clear()
    {
        _events.Clear();
        _pulls.Clear();
        _frames.Clear();
        _frameNames.Clear();
        _currentPull = 0;
        try { if (File.Exists(LogFilePath)) File.Delete(LogFilePath); }
        catch (Exception ex) { _log.Debug($"[YapYapDraw] log wipe failed: {ex.Message}"); }
    }

    // The captured log lives on disk so it survives plugin reloads and game
    // restarts; it only resets when Clear is pressed. Written on unload and
    // whenever a pull ends.
    private const uint LogFileMagic   = 0x59504C47; // "YPLG"
    private const int  LogFileVersion = 4;
    private string? _logPathCache;
    private string LogFilePath =>
        _logPathCache ??= Path.Combine(Plugin.PluginInterface.GetPluginConfigDirectory(), "session-log.bin");

    public void SaveToDisk()
    {
        try
        {
            Directory.CreateDirectory(Plugin.PluginInterface.GetPluginConfigDirectory());
            using var fs = new FileStream(LogFilePath, FileMode.Create, FileAccess.Write, FileShare.None);
            using var w  = new BinaryWriter(fs, Encoding.UTF8);
            w.Write(LogFileMagic);
            w.Write(LogFileVersion);
            w.Write(_seq);

            w.Write(_pulls.Count);
            foreach (var p in _pulls)
            {
                w.Write(p.Index);
                w.Write(p.Label);
                w.Write(p.Start.Ticks);
                w.Write(p.End.Ticks);
                w.Write(p.Events);
                w.Write(p.Territory);
                w.Write(p.MapId);
            }

            w.Write(_events.Count);
            foreach (var e in _events) WriteEvent(w, e);

            WriteFrames(w);
        }
        catch (Exception ex)
        {
            _log.Debug($"[YapYapDraw] log save failed: {ex.Message}");
        }
    }

    private void WriteFrames(BinaryWriter w)
    {
        int start = Math.Max(0, _frames.Count - MaxPersistFrames);
        w.Write(_frames.Count - start);
        for (int i = start; i < _frames.Count; i++)
        {
            var f = _frames[i];
            w.Write(f.Pull);
            w.Write(f.T);
            w.Write(f.Actors.Length);
            foreach (var s in f.Actors)
            {
                w.Write(s.Id);
                w.Write((byte)s.Kind);
                w.Write(s.X);
                w.Write(s.Z);
                w.Write(s.Rot);
                w.Write(s.HpPct);
                w.Write(s.CastId);
            }
        }

        w.Write(_frameNames.Count);
        foreach (var kv in _frameNames)
        {
            w.Write(kv.Key);
            w.Write(kv.Value);
        }
    }

    private void LoadFromDisk()
    {
        try
        {
            if (!File.Exists(LogFilePath)) return;
            using var fs = new FileStream(LogFilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            using var r  = new BinaryReader(fs, Encoding.UTF8);
            if (r.ReadUInt32() != LogFileMagic) return;
            int version = r.ReadInt32();
            if (version < 1 || version > LogFileVersion) return;
            _seq = r.ReadInt64();

            int pulls = r.ReadInt32();
            _pulls.Clear();
            for (int i = 0; i < pulls; i++)
            {
                var p = new PullInfo
                {
                    Index  = r.ReadInt32(),
                    Label  = r.ReadString(),
                    Start  = new DateTime(r.ReadInt64()),
                    End    = new DateTime(r.ReadInt64()),
                    Events = r.ReadInt32(),
                };
                if (version >= 3) p.Territory = r.ReadUInt32();
                if (version >= 4) p.MapId     = r.ReadUInt32();
                _pulls.Add(p);
            }

            int count = r.ReadInt32();
            _events.Clear();
            _events.Capacity = Math.Max(_events.Capacity, count);
            for (int i = 0; i < count; i++) _events.Add(ReadEvent(r));

            if (version >= 2) ReadFrames(r);

            // A fresh combat starts its own pull rather than appending to the
            // last restored one.
            _currentPull = 0;
        }
        catch (Exception ex)
        {
            _log.Debug($"[YapYapDraw] log load failed: {ex.Message}");
        }
    }

    private void ReadFrames(BinaryReader r)
    {
        int fc = r.ReadInt32();
        _frames.Clear();
        _frames.Capacity = Math.Max(_frames.Capacity, fc);
        for (int i = 0; i < fc; i++)
        {
            int pull = r.ReadInt32();
            double t = r.ReadDouble();
            int n    = r.ReadInt32();
            var arr  = new ActorSample[n];
            for (int j = 0; j < n; j++)
                arr[j] = new ActorSample(
                    r.ReadUInt32(), (ActorKind)r.ReadByte(),
                    r.ReadSingle(), r.ReadSingle(), r.ReadSingle(),
                    r.ReadSingle(), r.ReadUInt32());
            _frames.Add(new MapFrame { Pull = pull, T = t, Actors = arr });
        }

        int nameCount = r.ReadInt32();
        _frameNames.Clear();
        for (int i = 0; i < nameCount; i++)
        {
            uint id = r.ReadUInt32();
            _frameNames[id] = r.ReadString();
        }
    }

    private static void WriteEvent(BinaryWriter w, LogEvent e)
    {
        w.Write(e.Seq);
        w.Write(e.Time.Ticks);
        w.Write((byte)e.Kind);
        w.Write(e.Pull);
        w.Write(e.SourceName);
        w.Write(e.SourceId);
        w.Write((byte)e.SourceKind);
        w.Write(e.TargetName);
        w.Write(e.TargetId);
        w.Write((byte)e.TargetKind);
        w.Write(e.Name);
        w.Write(e.DataId);
        w.Write(e.IconId);
        w.Write(e.Value);
        w.Write(e.Count);
        w.Write(e.X);
        w.Write(e.Y);
        w.Write(e.Heading);
        w.Write(e.Category);
        w.Write(e.Param1);
        w.Write(e.Param2);
        w.Write(e.Param3);
        w.Write(e.Param4);
    }

    private static LogEvent ReadEvent(BinaryReader r) => new()
    {
        Seq        = r.ReadInt64(),
        Time       = new DateTime(r.ReadInt64()),
        Kind       = (LogKind)r.ReadByte(),
        Pull       = r.ReadInt32(),
        SourceName = r.ReadString(),
        SourceId   = r.ReadUInt32(),
        SourceKind = (ActorKind)r.ReadByte(),
        TargetName = r.ReadString(),
        TargetId   = r.ReadUInt32(),
        TargetKind = (ActorKind)r.ReadByte(),
        Name       = r.ReadString(),
        DataId     = r.ReadUInt32(),
        IconId     = r.ReadUInt32(),
        Value      = r.ReadSingle(),
        Count      = r.ReadUInt32(),
        X          = r.ReadSingle(),
        Y          = r.ReadSingle(),
        Heading    = r.ReadSingle(),
        Category   = r.ReadUInt32(),
        Param1     = r.ReadUInt32(),
        Param2     = r.ReadUInt32(),
        Param3     = r.ReadUInt32(),
        Param4     = r.ReadUInt32(),
    };

    public sealed class PullInfo
    {
        public int      Index;
        public string   Label   = "";
        public DateTime Start;
        public DateTime End;
        public int      Events;
        public uint     Territory;
        public uint     MapId;

        public string Duration()
        {
            var span = (End == DateTime.MinValue ? DateTime.Now : End) - Start;
            return span.TotalHours >= 1
                ? $"{(int)span.TotalMinutes}:{span.Seconds:00}"
                : $"{span.Minutes}:{span.Seconds:00}";
        }
    }

    private static unsafe uint CurrentMapId()
    {
        var am = AgentMap.Instance();
        return am != null ? am->CurrentMapId : 0;
    }

    public void NotifyCombat(bool inCombat)
    {
        if (inCombat == _inCombat) return;
        _inCombat = inCombat;

        if (inCombat)
        {
            bool freshPull = _currentPull == 0
                          || (DateTime.Now - _combatLeftAt).TotalSeconds > 8;
            if (freshPull)
            {
                _currentPull = _pulls.Count + 1;
                _pulls.Add(new PullInfo
                {
                    Index     = _currentPull,
                    Label     = $"Pull {_currentPull}",
                    Start     = DateTime.Now,
                    End       = DateTime.MinValue,
                    Territory = Plugin.ClientState.TerritoryType,
                    MapId     = CurrentMapId(),
                });
            }
        }
        else
        {
            _combatLeftAt = DateTime.Now;
            var p = _pulls.Find(x => x.Index == _currentPull);
            if (p != null) p.End = DateTime.Now;
            // Flush at the end of a pull so a crash mid-session keeps everything
            // up to the last fight.
            SaveToDisk();
        }
    }

    private sealed class ActorState
    {
        public uint                    LastCastId;
        public HashSet<(uint id, uint src)> Statuses = new();
        public bool                    Alive = true;
        public bool                    SeenThisPass;
    }

    public void Ingest(LogEvent e, bool addToLog = true) => Emit(e, addToLog);

    // Hands a hook-captured event to the framework tick for dispatch. Kept tiny so
    // the detour returns promptly and nothing heavy runs while the game is still
    // inside the hooked call.
    private void QueueFromHook(LogEvent e, bool addToLog = true)
    {
        lock (_hookQueueLock) _hookQueue.Enqueue((e, addToLog));
    }

    private void DrainHookQueue()
    {
        while (true)
        {
            (LogEvent e, bool addToLog) item;
            lock (_hookQueueLock)
            {
                if (_hookQueue.Count == 0) break;
                item = _hookQueue.Dequeue();
            }
            Emit(item.e, item.addToLog);
        }
    }

    public void Update()
    {
        DrainHookQueue();
        try
        {
            if (!ShouldCapture()) return;

            foreach (var state in _actors.Values) state.SeenThisPass = false;

            _eventObjScratch.Clear();
            foreach (var obj in Plugin.ObjectTable)
            {
                // Voidzone puddles/markers are EventObj actors, not IBattleChara.
                // Surface their spawn so draws can glue to them and vanish on despawn.
                if (obj.ObjectKind == Dalamud.Game.ClientState.Objects.Enums.ObjectKind.EventObj)
                {
                    var eid = obj.EntityId;
                    _eventObjScratch.Add(eid);
                    if (!_eventObjs.Contains(eid)) EmitAddedEventObj(obj);
                    continue;
                }
                if (obj is not IBattleChara bc) continue;
                var kind = Classify(bc);
                if (kind == ActorKind.Other) continue;
                Poll(bc, kind);
            }

            _eventObjs.Clear();
            foreach (var id in _eventObjScratch) _eventObjs.Add(id);

            var gone = _actors.Where(kv => !kv.Value.SeenThisPass).Select(kv => kv.Key).ToList();
            foreach (var id in gone) _actors.Remove(id);

            SampleFrame();
        }
        catch (Exception ex)
        {
            _log.Debug($"[YapYapDraw] capture error: {ex.Message}");
        }
    }

    private void SampleFrame()
    {
        // Only record while a pull is live and actually in combat — keeps the
        // replay timeline tight to the fight instead of trailing idle downtime.
        if (_currentPull == 0 || !_inCombat) return;
        var now = DateTime.Now;
        if ((now - _lastSnapshot).TotalSeconds < SnapshotInterval) return;
        _lastSnapshot = now;

        var pull = _pulls.Find(x => x.Index == _currentPull);
        if (pull == null) return;

        _frameScratch.Clear();
        foreach (var obj in Plugin.ObjectTable)
        {
            if (obj is not IBattleChara bc) continue;
            var kind = Classify(bc);
            if (kind == ActorKind.Other) continue;

            float hp = bc.MaxHp > 0 ? bc.CurrentHp / (float)bc.MaxHp * 100f : -1f;
            uint cast = bc.IsCasting ? bc.CastActionId : 0u;
            _frameScratch.Add(new ActorSample(
                bc.EntityId, kind, bc.Position.X, bc.Position.Z, bc.Rotation, hp, cast));

            if (!_frameNames.ContainsKey(bc.EntityId))
            {
                var nm = bc.Name.TextValue;
                if (!string.IsNullOrEmpty(nm)) _frameNames[bc.EntityId] = nm;
            }
        }

        if (_frameScratch.Count == 0) return;

        _frames.Add(new MapFrame
        {
            Pull   = _currentPull,
            T      = (now - pull.Start).TotalSeconds,
            Actors = _frameScratch.ToArray(),
        });

        if (_frames.Count > MaxFrames) _frames.RemoveRange(0, _frames.Count - MaxFrames);
    }

    public bool ShouldCapture() => _config.CaptureWhen switch
    {
        CaptureMode.InCombat => Plugin.Condition[Dalamud.Game.ClientState.Conditions.ConditionFlag.InCombat]
                                || Plugin.DutyState.IsDutyStarted,
        CaptureMode.InDuty   => Plugin.DutyState.IsDutyStarted,
        _                    => true,
    };

    private void Poll(IBattleChara bc, ActorKind kind)
    {
        if (!_actors.TryGetValue(bc.EntityId, out var state))
        {
            state = new ActorState { Alive = !(bc.IsDead || bc.CurrentHp == 0) };
            foreach (var s in bc.StatusList)
                if (s.StatusId != 0) state.Statuses.Add((s.StatusId, s.SourceId));
            _actors[bc.EntityId] = state;
            // A fresh enemy/object spawn carries the position that side-/count-based
            // mechanics (orbs, adds) read from an AddedCombatant line.
            if (kind == ActorKind.Enemy) EmitAdded(bc, kind);
            EmitSpawnTether(bc);
        }
        state.SeenThisPass = true;

        PollCast(bc, kind, state);
        PollStatuses(bc, kind, state);
        PollDeath(bc, kind, state);
    }

    // First sight of an actor: read the live VfxContainer's Tethers[0].
    private unsafe void EmitSpawnTether(IBattleChara bc)
    {
        var chr = (Character*)bc.Address;
        if (chr == null) return;

        var tether = chr->Vfx.Tethers[0];
        if (tether.Id == 0) return;

        NotifyTetherFromVfx(bc.EntityId, (uint)(ulong)tether.TargetId, tether.Id);
    }

    internal void NotifyTetherFromVfx(uint from, uint to, ushort tetherId)
    {
        if (!ShouldCapture() || to == 0xE0000000) return;

        var idx = _activeTethers.FindIndex(t => t.From == from && t.To == to);
        if (idx >= 0)
        {
            _activeTethers[idx] = (from, to, tetherId);
            return;
        }

        _activeTethers.Add((from, to, tetherId));
        QueueTether(from, to, tetherId);
    }

    internal void NotifyTetherCancelFromVfx(uint from)
    {
        if (!ShouldCapture()) return;

        _activeTethers.RemoveAll(t => t.From == from);
        QueueFromHook(new LogEvent
        {
            Kind     = LogKind.TetherCancel,
            SourceId = from,
            Name     = "Tether Cancel",
        });
    }

    private void QueueTether(uint from, uint to, ushort tetherId)
    {
        var fromObj = Plugin.ObjectTable.SearchById(from);
        var toObj   = to != 0 ? Plugin.ObjectTable.SearchById(to) : null;
        var kind    = fromObj is IBattleChara fbc ? Classify(fbc) : ActorKind.Other;

        QueueFromHook(new LogEvent
        {
            Kind       = LogKind.Tether,
            SourceId   = from,
            SourceName = fromObj?.Name.TextValue ?? "",
            SourceKind = kind,
            TargetId   = to,
            TargetName = toObj?.Name.TextValue ?? "",
            TargetKind = toObj is IBattleChara tbc ? Classify(tbc) : ActorKind.Other,
            DataId     = tetherId,
            Name       = $"Tether {tetherId:X4}",
        });
    }

    private void PollCast(IBattleChara bc, ActorKind kind, ActorState state)
    {
        uint cur = bc.IsCasting ? bc.CastActionId : 0u;
        uint prev = state.LastCastId;
        if (cur == prev) return;
        state.LastCastId = cur;

        // Cast bar emptied (resolved or interrupted) — the snapshot moment.
        if (prev != 0)
        {
            var done = Plugin.DataManager.GetExcelSheet<LuminaAction>().GetRowOrDefault(prev);
            var dpos = bc.Position;
            Emit(new LogEvent
            {
                Kind       = LogKind.CastFinish,
                SourceName = bc.Name.TextValue,
                SourceId   = bc.EntityId,
                SourceKind = kind,
                Name       = done?.Name.ExtractText() is { Length: > 0 } dn ? dn : $"#{prev}",
                DataId     = prev,
                IconId     = done?.Icon ?? 0,
                X          = dpos.X,
                Y          = dpos.Z,
                Heading    = bc.Rotation,
            });
        }

        if (cur == 0) return;

        // Enemy cast starts already arrive on the cast hook; only fill the gap for
        // the actors that hook skips (you / party / allies) so we don't double-log.
        if (CastHookInstalled && kind == ActorKind.Enemy) return;

        var action = Plugin.DataManager.GetExcelSheet<LuminaAction>().GetRowOrDefault(cur);
        var targetName = "";
        if (bc.CastTargetObjectId != 0 &&
            Plugin.ObjectTable.SearchById((uint)bc.CastTargetObjectId) is { } tgt)
            targetName = tgt.Name.TextValue;

        var pos = bc.Position;
        Emit(new LogEvent
        {
            Kind       = LogKind.CastStart,
            SourceName = bc.Name.TextValue,
            SourceId   = bc.EntityId,
            SourceKind = kind,
            TargetName = targetName,
            TargetId   = (uint)bc.CastTargetObjectId,
            Name       = action?.Name.ExtractText() is { Length: > 0 } n ? n : $"#{cur}",
            DataId     = cur,
            IconId     = action?.Icon ?? 0,
            Value      = bc.TotalCastTime,
            X          = pos.X,
            Y          = pos.Z,
            Heading    = bc.Rotation,
        });
    }

    private unsafe void ActorCastDetour(uint casterId, ActorCast* data)
    {
        _castHook!.Original(casterId, data);
        try
        {
            if (!ShouldCapture() || data == null || data->ActionKind != 1) return;
            if (Plugin.ObjectTable.SearchById(casterId) is not IBattleChara bc) return;
            if (Classify(bc) != ActorKind.Enemy) return;

            var pos = data->Pos;
            Data.LastCastPositions[casterId] = pos;

            float heading = ((Character*)bc.Address)->CastRotation;

            var action = Plugin.DataManager.GetExcelSheet<LuminaAction>().GetRowOrDefault(data->ActionId);
            var targetName = "";
            if (data->TargetId != 0 && Plugin.ObjectTable.SearchById(data->TargetId) is { } tgt)
                targetName = tgt.Name.TextValue;

            QueueFromHook(new LogEvent
            {
                Kind       = LogKind.CastStart,
                SourceName = bc.Name.TextValue,
                SourceId   = casterId,
                SourceKind = ActorKind.Enemy,
                TargetName = targetName,
                TargetId   = data->TargetId,
                Name       = action?.Name.ExtractText() is { Length: > 0 } n ? n : $"#{data->ActionId}",
                DataId     = data->ActionId,
                IconId     = action?.Icon ?? 0,
                Value      = data->CastTime + 0.3f,
                Param1     = data->DisplayDelay,
                X          = pos.X,
                Y          = pos.Z,
                Heading    = heading,
            });
        }
        catch (Exception ex)
        {
            _log.Debug($"[YapYapDraw] actor-cast error: {ex.Message}");
        }
    }

    private void PollStatuses(IBattleChara bc, ActorKind kind, ActorState state)
    {
        var current = _statusScratch;
        current.Clear();
        foreach (var s in bc.StatusList)
            if (s.StatusId != 0) current.Add((s.StatusId, s.SourceId));

        foreach (var c in current)
            if (!state.Statuses.Contains(c)) EmitStatus(LogKind.StatusGain, bc, kind, c);

        foreach (var c in state.Statuses)
            if (!current.Contains(c)) EmitStatus(LogKind.StatusLose, bc, kind, c);

        state.Statuses.Clear();
        foreach (var c in current) state.Statuses.Add(c);
    }

    private void EmitStatus(LogKind kind, IBattleChara target, ActorKind targetKind, (uint id, uint src) s)
    {
        var status = Plugin.DataManager.GetExcelSheet<LuminaStatus>().GetRowOrDefault(s.id);
        var srcName = s.src != 0 && Plugin.ObjectTable.SearchById(s.src) is { } src ? src.Name.TextValue : "";
        float remaining = 0f;
        uint  param     = 0;
        if (kind == LogKind.StatusGain)
        {
            var live = target.StatusList.FirstOrDefault(x => x.StatusId == s.id);
            if (live != null) { remaining = live.RemainingTime; param = live.Param; }
        }

        Emit(new LogEvent
        {
            Kind       = kind,
            SourceName = srcName,
            SourceId   = s.src,
            SourceKind = ActorKind.Other,
            TargetName = target.Name.TextValue,
            TargetId   = target.EntityId,
            TargetKind = targetKind,
            Name       = status?.Name.ExtractText() is { Length: > 0 } n ? n : $"#{s.id}",
            DataId     = s.id,
            IconId     = status?.Icon ?? 0,
            Value      = remaining,
            Count      = param,
        });
    }

    private void EmitAdded(IBattleChara bc, ActorKind kind)
    {
        var pos = bc.Position;
        Emit(new LogEvent
        {
            Kind       = LogKind.Added,
            SourceName = bc.Name.TextValue,
            SourceId   = bc.EntityId,
            SourceKind = kind,
            TargetName = bc.Name.TextValue,
            TargetId   = bc.EntityId,
            Name       = bc.Name.TextValue,
            DataId     = bc.BaseId,
            X          = pos.X,
            Y          = pos.Z,
        }, addToLog: _config.ShowAdds);
    }

    private void EmitAddedEventObj(IGameObject obj)
    {
        var pos = obj.Position;
        Emit(new LogEvent
        {
            Kind       = LogKind.Added,
            SourceName = obj.Name.TextValue,
            SourceId   = obj.EntityId,
            SourceKind = ActorKind.Other,
            TargetName = obj.Name.TextValue,
            TargetId   = obj.EntityId,
            Name       = obj.Name.TextValue,
            DataId     = obj.DataId,
            X          = pos.X,
            Y          = pos.Z,
        }, addToLog: _config.ShowAdds);
    }

    private void PollDeath(IBattleChara bc, ActorKind kind, ActorState state)
    {
        bool dead = bc.IsDead || bc.CurrentHp == 0;
        if (dead && state.Alive)
            Emit(new LogEvent
            {
                Kind       = LogKind.Death,
                SourceName = bc.Name.TextValue,
                SourceId   = bc.EntityId,
                SourceKind = kind,
                TargetName = bc.Name.TextValue,
                TargetId   = bc.EntityId,
                Name       = "Death",
            });
        state.Alive = !dead;
    }

    // Append a free-text diagnostic line (e.g. JS console.log output) straight to
    // the fight log so it shows in the log window and exports. Intentionally does
    // NOT raise OnEvent: these are written from inside JS execution, and feeding
    // them back into the trigger engine would re-enter the (non-reentrant) JS host.
    public void Note(string msg)
    {
        var e = new LogEvent
        {
            Seq        = ++_seq,
            Time       = DateTime.Now,
            Pull       = _currentPull,
            Kind       = LogKind.Note,
            SourceKind = ActorKind.Other,
            Name       = msg ?? "",
        };
        TotalEmitted++;
        LastEventAt = e.Time;
        _kindCounts[(int)LogKind.Note]++;
        _events.Add(e);
        if (_events.Count > 200000) _events.RemoveRange(0, _events.Count - 200000);
        var p = _pulls.Find(x => x.Index == _currentPull);
        if (p != null) p.Events++;
    }

    private void Emit(LogEvent e, bool addToLog = true)
    {
        e = e with { Seq = ++_seq, Time = DateTime.Now, Pull = _currentPull };
        TotalEmitted++;
        LastEventAt = e.Time;
        if ((int)e.Kind < _kindCounts.Length) _kindCounts[(int)e.Kind]++;
        if (addToLog)
        {
            _events.Add(e);
            // Keep a whole fight's worth of events so nothing is lost before an
            // export. The trim only kicks in for marathon sessions.
            if (_events.Count > 200000) _events.RemoveRange(0, _events.Count - 200000);
            var p = _pulls.Find(x => x.Index == _currentPull);
            if (p != null) p.Events++;
        }
        try { OnEvent?.Invoke(e); } catch (Exception ex) { _log.Debug($"[YapYapDraw] trigger error: {ex.Message}"); }
    }

    private unsafe void ActionEffectDetour(
        uint casterEntityId, Character* casterPtr, Vector3* targetPos,
        ActionEffectHandler.Header* header, ActionEffectHandler.TargetEffects* effects,
        GameObjectId* targetEntityIds)
    {
        _actionEffectHook!.Original(casterEntityId, casterPtr, targetPos, header, effects, targetEntityIds);

        try
        {
            if (!ShouldCapture()) return;
            uint actionId = header->SpellId;
            if (actionId == 0) return;
            if (Plugin.ObjectTable.SearchById(casterEntityId) is not IGameObject caster) return;
            if (caster is not IBattleChara bc) return;

            var kind = Classify(bc);
            if (kind == ActorKind.Other) return;

            var action = Plugin.DataManager.GetExcelSheet<LuminaAction>().GetRowOrDefault(actionId);
            if (action is not { } a) return;
            if (a.ActionCategory.RowId == 1) return;
            var name = a.Name.ExtractText();
            if (string.IsNullOrEmpty(name)) name = $"#{actionId}";

            var targetCount = Math.Min((int)header->NumTargets, 32);
            var abilityTargetIds = new uint[targetCount];
            for (var i = 0; i < targetCount; i++)
                abilityTargetIds[i] = (uint)(targetEntityIds[i] & uint.MaxValue);

            uint firstTarget = targetCount > 0 ? abilityTargetIds[0] : 0u;
            string tname = firstTarget != 0 && Plugin.ObjectTable.SearchById(firstTarget) is { } tgt
                ? tgt.Name.TextValue : "";

            QueueFromHook(new LogEvent
            {
                Kind       = LogKind.Ability,
                SourceName = caster.Name.TextValue,
                SourceId   = caster.EntityId,
                SourceKind = kind,
                TargetName = tname,
                TargetId   = firstTarget,
                Name       = name,
                DataId     = actionId,
                IconId     = a.Icon,
                X          = caster.Position.X,
                Y          = caster.Position.Z,
                Heading    = caster.Rotation,
                AbilityTargetIds = abilityTargetIds,
            });

            // AbilityExtra carries the effect's ground position, which some
            // mechanics (e.g. the mana-sphere puzzle) read to place callouts.
            if (targetPos != null)
                QueueFromHook(new LogEvent
                {
                    Kind       = LogKind.AbilityExtra,
                    SourceId   = caster.EntityId,
                    SourceName = caster.Name.TextValue,
                    SourceKind = kind,
                    Name       = name,
                    DataId     = actionId,
                    X          = targetPos->X,
                    Y          = targetPos->Z,
                }, _config.ShowPositions);
        }
        catch (Exception ex)
        {
            _log.Debug($"[YapYapDraw] action-effect error: {ex.Message}");
        }
    }

    private void ActorControlDetour(
        uint actorId, uint category, uint p1, uint p2, uint p3, uint p4,
        uint p5, uint p6, uint p7, uint p8, GameObjectId targetId, bool replaying)
    {
        _actorControlHook!.Original(actorId, category, p1, p2, p3, p4, p5, p6, p7, p8, targetId, replaying);

        try
        {
            if (!ShouldCapture()) return;

            if (category == CategoryTargetIcon)
            {
                if (p2 == 0) _activeHeadmarkers.Remove(actorId);
                else _activeHeadmarkers[actorId] = p2;

                var aObj = Plugin.ObjectTable.SearchById(actorId);
                var name = aObj?.Name.TextValue ?? "";
                QueueFromHook(new LogEvent
                {
                    Kind       = LogKind.Headmarker,
                    SourceId   = actorId,
                    SourceName = name,
                    SourceKind = aObj is IBattleChara abc ? Classify(abc) : ActorKind.Other,
                    DataId     = p2,
                    Param1     = p3,
                    Name       = $"Headmarker {p2:X4}",
                });
            }
            else if (category == CategoryPlayActionTimeline)
            {
                // Single play-action-timeline event (e.g. clone spawn/transform
                // animations 0x11D2 / 0x1E46); p1 is the timeline row id.
                var sObj = Plugin.ObjectTable.SearchById(actorId);
                QueueFromHook(new LogEvent
                {
                    Kind       = LogKind.TimelineEvent,
                    SourceId   = actorId,
                    SourceName = sObj?.Name.TextValue ?? "",
                    SourceKind = sObj is IBattleChara tbc2 ? Classify(tbc2) : ActorKind.Other,
                    DataId     = p1,
                    Name       = $"Timeline {p1:X4}",
                }, _config.ShowControl);
            }
            else if (category == CategoryEventObjectAnim)
            {
                QueueFromHook(new LogEvent
                {
                    Kind     = LogKind.EventObject,
                    SourceId = actorId,
                    Param1   = p1,
                    Param2   = p2,
                    Name     = $"EventObject {p1}/{p2}",
                });
            }
            else
            {
                // Generic control op (e.g. category 0197 / director commands). These
                // drive ActorControl/ActorControlExtra triggers. Logged only when the
                // author opts in (Ctrl filter), since they can be high volume.
                var sObj = Plugin.ObjectTable.SearchById(actorId);
                QueueFromHook(new LogEvent
                {
                    Kind       = LogKind.ActorControl,
                    SourceId   = actorId,
                    SourceName = sObj?.Name.TextValue ?? "",
                    SourceKind = sObj is IBattleChara scbc ? Classify(scbc) : ActorKind.Other,
                    Name       = "ActorControl",
                    Category   = category,
                    Param1     = p1,
                    Param2     = p2,
                    Param3     = p3,
                    Param4     = p4,
                }, _config.ShowControl);
            }
        }
        catch (Exception ex)
        {
            _log.Debug($"[YapYapDraw] actor-control error: {ex.Message}");
        }
    }

    private static ActorKind Classify(IBattleChara bc)
    {
        if (bc.EntityId == Plugin.PlayerState.EntityId) return ActorKind.You;

        if (bc.ObjectKind == Dalamud.Game.ClientState.Objects.Enums.ObjectKind.Pc) return ActorKind.Party;
        if (bc.ObjectKind == Dalamud.Game.ClientState.Objects.Enums.ObjectKind.BattleNpc)
        {
            if (bc is IBattleNpc bn && IsFriendlyNpc((byte)bn.BattleNpcKind))
                return ActorKind.Party;
            return ActorKind.Enemy;
        }
        return ActorKind.Other;
    }

    // BattleNpcSubKind: Pet=2, Chocobo=3, NpcPartyMember=9 are owned/allied, not
    // hostiles. Compared by value to stay independent of the enum binding.
    private static bool IsFriendlyNpc(byte kind) => kind is 2 or 3 or 9;
}
