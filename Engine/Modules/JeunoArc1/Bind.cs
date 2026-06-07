using System;
using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;
using YapYapDraw.Engine.Util;
using YapYapDraw.Engine.Vfx;

namespace YapYapDraw.Modules.JeunoArc1;

public class Bind : ISpecialAction
{
    public override string Name => "Bind";

    public override HashSet<uint> ActionID => new HashSet<uint> { 41513u, 41514u };

    public override uint Phase => 4u;

    public override IEnumerable<StaticVfx> ActiveAOEs
    {
        get
        {
            int count = aoes.Count;
            if (count == 0)
            {
                return Array.Empty<StaticVfx>();
            }
            long drawTime = aoes[0].DrawTime;
            List<StaticVfx> active = new List<StaticVfx>();
            for (int i = 0; i < count; i++)
            {
                if (aoes[i].DrawTime - drawTime < 100)
                {
                    active.Add(aoes[i]);
                }
            }
            return active;
        }
    }

    public override void OnActionCast(ActorCastInfo info)
    {
        if (info.ActionId == 41513)
        {
            aoes.Add(SimpleElement.Circle(info.SourceId.GameObject().Position, 9f, Environment.TickCount64));
        }
    }

    public override void OnAbilityCast(ActorAbilityInfo info)
    {
        if (info.ActionId == 41514 && aoes.Count != 0)
        {
            aoes[0].Remove();
            aoes.RemoveAt(0);
        }
    }
}
