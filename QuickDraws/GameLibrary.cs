using System;
using System.Collections.Generic;
using LuminaAction = Lumina.Excel.Sheets.Action;
using LuminaStatus = Lumina.Excel.Sheets.Status;

namespace YapYapDraw.QuickDraws;

public static class GameLibrary
{
    public readonly record struct Entry(uint Id, string Name, uint Icon, bool IsStatus);

    private static List<Entry>? _all;

    private static void Ensure()
    {
        if (_all != null) return;
        var list = new List<Entry>(8192);

        foreach (var a in Plugin.Actions)
        {
            var name = a.Name.ExtractText();
            if (string.IsNullOrWhiteSpace(name)) continue;
            list.Add(new Entry(a.RowId, name, a.Icon, false));
        }

        foreach (var s in Plugin.Statuses)
        {
            var name = s.Name.ExtractText();
            if (string.IsNullOrWhiteSpace(name)) continue;
            list.Add(new Entry(s.RowId, name, s.Icon, true));
        }

        _all = list;
    }

    public static List<Entry> Search(string query, int limit = 40)
    {
        Ensure();
        var results = new List<Entry>(limit);
        if (string.IsNullOrWhiteSpace(query)) return results;

        var exact  = new List<Entry>();
        var starts = new List<Entry>();
        var has    = new List<Entry>();

        foreach (var e in _all!)
        {
            int idx = e.Name.IndexOf(query, StringComparison.OrdinalIgnoreCase);
            if (idx < 0) continue;
            if (e.Name.Length == query.Length) exact.Add(e);
            else if (idx == 0) starts.Add(e);
            else has.Add(e);
        }

        foreach (var bucket in new[] { exact, starts, has })
        {
            foreach (var e in bucket)
            {
                if (results.Count >= limit) return results;
                results.Add(e);
            }
        }
        return results;
    }
}
