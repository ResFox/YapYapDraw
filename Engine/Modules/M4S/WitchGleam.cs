using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using YapYapDraw.Engine.Interop.ActionEffect;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.M4S;

public class WitchGleam : ISpecialAction
{
    public static int[] Stacks = new int[8];

    public override string Name => "Ion Cluster Gleam";

    public override uint Phase => 2u;

    public override HashSet<uint> ActionID => new HashSet<uint> { 38789u, 38790u };

    public static List<IGameObject> Players => PlayerHelper.AllPlayers;

    public override void OnAbilityCast(ActorAbilityInfo info)
    {
        if (info.ActionId == 38789)
        {
            Stacks = new int[8];
        }
        if (info.ActionId != 38790)
        {
            return;
        }
        int index = -1;
        TargetEffect[] targetEffects = info.TargetEffects;
        for (int i = 0; i < targetEffects.Length; i++)
        {
            TargetEffect effect = targetEffects[i];
            index = Players.FindIndex((IGameObject x) => x.EntityId == effect.TargetID);
        }
        if (index >= 0)
        {
            Stacks[index]++;
        }
    }

    public override void Reset()
    {
        Stacks = new int[8];
    }
}
