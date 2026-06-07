using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Dalamud.Plugin.Services;
using YapYapDraw.Logging;

namespace YapYapDraw.QuickDraws;

// Per-zone record of the casts, statuses, headmarkers and tethers seen so far.
public sealed class FightCatalog
{
    public enum Kind : byte { Cast, Status, Headmarker, Tether }

    public sealed class Entry
    {
        public Kind   Kind { get; set; }
        public uint   Id   { get; set; }
        public string Name { get; set; } = "";
        public uint   Icon { get; set; }
    }

    private sealed class ZoneData
    {
        public uint        Territory { get; set; }
        public List<Entry> Entries   { get; set; } = new();
    }

    private sealed class Store
    {
        public int            Version { get; set; }
        public List<ZoneData> Zones   { get; set; } = new();
    }

    private const int StoreVersion = 1;

    private readonly Dictionary<uint, Dictionary<long, Entry>> _byZone = new();
    private readonly string     _path;
    private readonly IPluginLog _log;
    private bool     _dirty;
    private DateTime _lastSave = DateTime.MinValue;

    public FightCatalog(string dir, IPluginLog log)
    {
        _log  = log;
        _path = Path.Combine(dir, "catalog.json");
        Load();
    }

    private static long Key(Kind kind, uint id) => ((long)(byte)kind << 32) | id;

    private static bool SourceIsHostile(uint srcId)
    {
        if (srcId == 0) return false;
        var obj = Plugin.ObjectTable.SearchById(srcId);
        if (obj == null) return true;
        if (obj.ObjectKind == Dalamud.Game.ClientState.Objects.Enums.ObjectKind.Pc) return false;
        if (obj is Dalamud.Game.ClientState.Objects.Types.IBattleNpc bn)
            return (byte)bn.BattleNpcKind is not (2 or 3 or 9);
        return true;
    }

    public void Record(LogEvent e)
    {
        uint terr = Plugin.ClientState.TerritoryType;
        if (terr == 0) return;

        Kind kind;
        switch (e.Kind)
        {
            case LogKind.CastStart:
                if (e.SourceKind != ActorKind.Enemy) return;
                kind = Kind.Cast; break;
            case LogKind.StatusGain:
                if (!SourceIsHostile(e.SourceId)) return;
                kind = Kind.Status; break;
            case LogKind.Headmarker: kind = Kind.Headmarker; break;
            case LogKind.Tether:     kind = Kind.Tether;     break;
            default: return;
        }

        if (e.DataId == 0 && string.IsNullOrEmpty(e.Name)) return;

        if (!_byZone.TryGetValue(terr, out var map)) { map = new(); _byZone[terr] = map; }
        var k = Key(kind, e.DataId);
        if (map.ContainsKey(k)) return;

        map[k] = new Entry { Kind = kind, Id = e.DataId, Name = e.Name, Icon = e.IconId };
        _dirty = true;
    }

    public List<uint> Zones() => new(_byZone.Keys);

    public List<Entry> Entries(uint territory)
        => _byZone.TryGetValue(territory, out var m) ? new List<Entry>(m.Values) : new();

    public int Count(uint territory)
        => _byZone.TryGetValue(territory, out var m) ? m.Count : 0;

    public void Clear(uint territory)
    {
        if (_byZone.Remove(territory)) { _dirty = true; Save(); }
    }

    public void MaybeSave()
    {
        if (!_dirty) return;
        if ((DateTime.Now - _lastSave).TotalSeconds < 5) return;
        Save();
    }

    public void Save()
    {
        try
        {
            var store = new Store { Version = StoreVersion };
            foreach (var (terr, map) in _byZone)
                store.Zones.Add(new ZoneData { Territory = terr, Entries = new List<Entry>(map.Values) });
            File.WriteAllText(_path, JsonSerializer.Serialize(store));
            _dirty    = false;
            _lastSave = DateTime.Now;
        }
        catch (Exception ex) { _log.Debug($"[YapYapDraw] catalog save: {ex.Message}"); }
    }

    private void Load()
    {
        try
        {
            if (!File.Exists(_path)) return;
            var store = JsonSerializer.Deserialize<Store>(File.ReadAllText(_path));
            if (store?.Zones == null) return;
            if (store.Version < StoreVersion) { _dirty = true; return; }
            foreach (var z in store.Zones)
            {
                var map = new Dictionary<long, Entry>();
                foreach (var en in z.Entries) map[Key(en.Kind, en.Id)] = en;
                _byZone[z.Territory] = map;
            }
        }
        catch (Exception ex) { _log.Debug($"[YapYapDraw] catalog load: {ex.Message}"); }
    }
}
