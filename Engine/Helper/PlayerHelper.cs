using System;
using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using YapYapDraw.Engine;
using YapYapDraw.Engine.Interop;
using YapYapDraw.Engine.Interop.Game;

namespace YapYapDraw.Engine.Helper;

public static class PlayerHelper
{
    public static List<IGameObject> AllPlayers => Svc.Objects.Where((IGameObject o) => (int)o.ObjectKind == 1).ToList();

    public static List<IGameObject> Tank => Svc.Objects.Where(o =>
    {
        if ((int)o.ObjectKind == 1 && o is IPlayerCharacter pc)
        {
            return ((ICharacter)pc).GetRole() == CombatRole.Tank;
        }
        return false;
    }).ToList();

    public static List<IGameObject> Healer => Svc.Objects.Where(o =>
    {
        if ((int)o.ObjectKind == 1 && o is IPlayerCharacter pc)
        {
            return ((ICharacter)pc).GetRole() == CombatRole.Healer;
        }
        return false;
    }).ToList();

    public static List<IGameObject> DPS => Svc.Objects.Where(o =>
    {
        if ((int)o.ObjectKind == 1 && o is IPlayerCharacter pc)
        {
            return ((ICharacter)pc).GetRole() == CombatRole.DPS;
        }
        return false;
    }).ToList();

    public static float CameraDirHToCharaRotation(float cameraDirH)
    {
        return (cameraDirH - (float)Math.PI) % ((float)Math.PI * 2f);
    }

    public static IEnumerable<IGameObject> RaidByEnmity(ulong primaryTargetId, bool allowGuessing = true)
    {
        var enmity = FightClientState.CurrentTargetEnmity;
        FightClientState.EnmityEntry[]? entries = null;
        if (enmity.TargetId == primaryTargetId)
            entries = enmity.Entries;
        else if (FightClientState.TryGetEnmity(primaryTargetId, out var cached))
            entries = cached;

        if (entries != null)
        {
            var ordered = entries
                .Where(h => h.EntityId != 0)
                .OrderByDescending(h => h.Enmity)
                .Select(h => Svc.Objects.SearchById(h.EntityId))
                .Where(o => o != null)
                .Cast<IGameObject>()
                .ToList();
            if (ordered.Count > 0)
                return ordered;
        }

        if (!allowGuessing)
            return Array.Empty<IGameObject>();

        ulong bossTargetId = Svc.Objects.SearchById(primaryTargetId)?.TargetObject?.GameObjectId ?? 0;
        return AllPlayers.OrderBy(p => GuessEnmityOrder(p, bossTargetId));
    }

    private static int GuessEnmityOrder(IGameObject player, ulong bossTargetId)
    {
        if (player.GameObjectId == bossTargetId)
            return 0;

        if (player is IPlayerCharacter pc)
        {
            return ((ICharacter)pc).GetRole() switch
            {
                CombatRole.Tank => 1,
                CombatRole.Healer => 3,
                _ => 2
            };
        }

        return 4;
    }
}
