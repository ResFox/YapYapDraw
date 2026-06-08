using System;
using FFXIVClientStructs.FFXIV.Client.Game.UI;

namespace YapYapDraw.Engine;

internal static class FightClientState
{
    public const int NumHateTargets = 32;

    public readonly struct HateEntry(ulong entityId, int enmity)
    {
        public readonly ulong EntityId = entityId;
        public readonly int Enmity = enmity;
    }

    public readonly struct TargetHate(ulong targetId, HateEntry[] entries)
    {
        public readonly ulong TargetId = targetId;
        public readonly HateEntry[] Entries = entries;
    }

    public static TargetHate CurrentTargetHate { get; private set; } =
        new TargetHate(0, new HateEntry[NumHateTargets]);

    public static unsafe void PollHate()
    {
        var ui = UIState.Instance();
        if (ui == null)
            return;

        ref var hate = ref ui->Hate;
        uint targetId = hate.HateTargetId;
        var entries = new HateEntry[NumHateTargets];
        int len = Math.Min(hate.HateArrayLength, NumHateTargets);
        for (int i = 0; i < len; i++)
        {
            entries[i] = new HateEntry(hate.HateInfo[i].EntityId, hate.HateInfo[i].Enmity);
        }

        if (targetId == CurrentTargetHate.TargetId && entries.AsSpan().SequenceEqual(CurrentTargetHate.Entries.AsSpan()))
            return;

        CurrentTargetHate = new TargetHate(targetId, entries);
    }
}
