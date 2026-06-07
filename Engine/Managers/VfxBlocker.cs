using System.Collections.Generic;
using Dalamud.Game;
using YapYapDraw.Engine.Interop;
using Lumina.Excel.Sheets;
using Lumina.Text.ReadOnly;
using YapYapDraw.Engine.Helper;
using Action = Lumina.Excel.Sheets.Action;

namespace YapYapDraw.Engine.Managers;

public static class VfxBlocker
{
    public static readonly List<string> BlockedPaths = new();

    private static readonly HashSet<string> Synced = new();
    private static uint _lastWeather;
    private static uint _lastGroupId;

    public static void Dispose() => ClearSyncedBlocks();

    public static void SyncOmenBlocks(
        IEnumerable<Dictionary<uint, HashSet<uint>>> blockOmenMaps,
        IEnumerable<Dictionary<uint, HashSet<string>>> blockOmenPathMaps,
        uint weatherId,
        uint moduleGroupId)
    {
        if (weatherId == _lastWeather && moduleGroupId == _lastGroupId)
            return;

        foreach (var path in Synced)
            BlockedPaths.Remove(path);
        Synced.Clear();
        _lastWeather = weatherId;
        _lastGroupId = moduleGroupId;

        foreach (var blockOmenMap in blockOmenMaps)
        {
            if (blockOmenMap.Count == 0
                || (!blockOmenMap.TryGetValue(weatherId, out var actionIds)
                    && !blockOmenMap.TryGetValue(0u, out actionIds)))
                continue;

            foreach (var actionId in actionIds)
            {
                var row = Svc.Data.GetExcelSheet<Action>((ClientLanguage?)null, (string?)null).GetRow(actionId);
                if (!row.Omen.IsValid) continue;
                var text = row.Omen.Value.Path.ExtractText();
                if (string.IsNullOrEmpty(text)) continue;
                AddBlocked(text.Omen());
            }
            break;
        }

        foreach (var blockOmenPathMap in blockOmenPathMaps)
        {
            if (blockOmenPathMap.Count == 0
                || (!blockOmenPathMap.TryGetValue(weatherId, out var paths)
                    && !blockOmenPathMap.TryGetValue(0u, out paths)))
                continue;

            foreach (var path in paths)
                AddBlocked(path);
            break;
        }
    }

    public static void ClearSyncedBlocks()
    {
        foreach (var path in Synced)
            BlockedPaths.Remove(path);
        Synced.Clear();
        _lastWeather = 0;
        _lastGroupId = 0;
    }

    private static void AddBlocked(string path)
    {
        if (string.IsNullOrEmpty(path) || Synced.Contains(path)) return;
        Synced.Add(path);
        if (!BlockedPaths.Contains(path))
            BlockedPaths.Add(path);
    }
}
