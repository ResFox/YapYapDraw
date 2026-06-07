using System;
using System.Collections.Generic;
using LuminaCfc  = Lumina.Excel.Sheets.ContentFinderCondition;
using LuminaTerr = Lumina.Excel.Sheets.TerritoryType;

namespace YapYapDraw.QuickDraws;

// Lets quick draws be scoped to duties. Users think in duty names ("M12S"), so we
// search ContentFinderCondition and map each to its TerritoryType id, which is
// what ClientState.TerritoryType reports in-game.
public static class ZoneLibrary
{
    public readonly record struct Zone(uint TerritoryId, string Name);

    private static List<Zone>?               _all;
    private static Dictionary<uint, string>? _names;
    private static Dictionary<uint, string>? _cats;

    private static void Ensure()
    {
        if (_all != null) return;
        var list  = new List<Zone>(2048);
        var names = new Dictionary<uint, string>();
        var cats  = new Dictionary<uint, string>();

        foreach (var c in Plugin.DataManager.GetExcelSheet<LuminaCfc>())
        {
            var name = c.Name.ExtractText();
            if (string.IsNullOrWhiteSpace(name)) continue;
            uint terr = c.TerritoryType.RowId;
            if (terr == 0) continue;
            list.Add(new Zone(terr, name));
            names[terr] = name;

            string cat = "";
            try { cat = c.ContentType.Value.Name.ExtractText(); } catch { }
            cats[terr] = string.IsNullOrWhiteSpace(cat) ? "Other" : cat;
        }

        _all   = list;
        _names = names;
        _cats  = cats;
    }

    public static string CategoryOf(uint territoryId)
    {
        Ensure();
        return _cats!.TryGetValue(territoryId, out var c) ? c : "Other";
    }

    public static string NameOf(uint territoryId)
    {
        Ensure();
        if (territoryId == 0) return "Open world / unknown";
        if (_names!.TryGetValue(territoryId, out var n)) return n;
        try
        {
            var t  = Plugin.DataManager.GetExcelSheet<LuminaTerr>().GetRowOrDefault(territoryId);
            var pn = t?.PlaceName.Value.Name.ExtractText();
            if (!string.IsNullOrWhiteSpace(pn)) return pn!;
        }
        catch { }
        return $"Zone {territoryId}";
    }

    public static List<Zone> Search(string query, int limit = 30)
    {
        Ensure();
        var results = new List<Zone>();
        if (string.IsNullOrWhiteSpace(query)) return results;

        foreach (var z in _all!)
        {
            if (z.Name.IndexOf(query, StringComparison.OrdinalIgnoreCase) < 0) continue;
            results.Add(z);
            if (results.Count >= limit) break;
        }
        return results;
    }
}
