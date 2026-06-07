using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;
using YapYapDraw.Engine.Util;
using YapYapDraw.Engine.Vfx;

namespace YapYapDraw.Modules.JeunoArc1;

public class ArkDominion : ISpecialAction
{
    public override string Name => "Ark Dominion";

    public override HashSet<uint> ActionID => new HashSet<uint> { 41094u, 40628u };

    public override uint Phase => 3u;

    public override void OnEventObjectAnimation(uint actorID, ushort p1, ushort p2)
    {
        if (actorID.GameObject().BaseId == 2014407 && p1 == 1 && p2 == 2)
        {
            aoes.Add(SimpleElement.Circle(actorID.GameObject().Position, 6f, 30000f));
        }
    }

    public override void OnAbilityCast(ActorAbilityInfo info)
    {
        foreach (StaticVfx aoe in aoes)
        {
            if (aoe.Position.AlmostEqual(info.Source.Position, 1f))
            {
                aoe.Remove();
            }
        }
        aoes.RemoveAll((StaticVfx aoe) => aoe.Position.AlmostEqual(info.Source.Position, 1f));
    }
}
