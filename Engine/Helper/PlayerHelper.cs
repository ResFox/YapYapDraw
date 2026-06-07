using System;
using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
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
}
