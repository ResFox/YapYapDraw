using System;
using System.Collections.Generic;
using FFXIVClientStructs.FFXIV.Client.Game.UI;

namespace YapYapDraw.Engine;

internal static class FightClientState
{
    public const int NumEnmityTargets = 32;

    public readonly struct EnmityEntry(ulong entityId, int enmity)
    {
        public readonly ulong EntityId = entityId;
        public readonly int Enmity = enmity;
    }

    public readonly struct TargetEnmity(ulong targetId, EnmityEntry[] entries)
    {
        public readonly ulong TargetId = targetId;
        public readonly EnmityEntry[] Entries = entries;
    }

    public static TargetEnmity CurrentTargetEnmity { get; private set; } =
        new TargetEnmity(0, new EnmityEntry[NumEnmityTargets]);

    // cache per enemy so ordering still works when it isn't your current target
    private static readonly Dictionary<ulong, EnmityEntry[]> EnmityByTarget = new();

    public static bool TryGetEnmity(ulong targetId, out EnmityEntry[] entries)
        => EnmityByTarget.TryGetValue(targetId, out entries!);

    public static void ClearEnmity() => EnmityByTarget.Clear();

    public static unsafe void PollEnmity()
    {
        var ui = UIState.Instance();
        if (ui == null)
            return;

        ref var hate = ref ui->Hate;
        uint targetId = hate.HateTargetId;
        var entries = new EnmityEntry[NumEnmityTargets];
        int len = Math.Min(hate.HateArrayLength, NumEnmityTargets);
        for (int i = 0; i < len; i++)
        {
            entries[i] = new EnmityEntry(hate.HateInfo[i].EntityId, hate.HateInfo[i].Enmity);
        }

        if (targetId != 0)
            EnmityByTarget[targetId] = entries;

        if (targetId == CurrentTargetEnmity.TargetId && entries.AsSpan().SequenceEqual(CurrentTargetEnmity.Entries.AsSpan()))
            return;

        CurrentTargetEnmity = new TargetEnmity(targetId, entries);
    }
}
