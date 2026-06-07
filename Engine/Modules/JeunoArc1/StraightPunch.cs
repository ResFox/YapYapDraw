using System.Collections.Generic;
using System.Linq;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;
using YapYapDraw.Engine.Util;
using YapYapDraw.Engine.Vfx;

namespace YapYapDraw.Modules.JeunoArc1;

public class StraightPunch : ISpecialAction
{
    public override string Name => "Straight Punch";

    public override HashSet<uint> ActionID => new HashSet<uint> { 40939u, 40940u, 40941u, 40942u, 40943u, 40944u };

    public override IEnumerable<StaticVfx> ActiveAOEs => aoes.Take(1);

    public override void OnActionCast(ActorCastInfo info)
    {
        switch (info.ActionId)
        {
        case 40939:
            aoes.Add(SimpleElement.Circle(info, 9f));
            break;
        case 40940:
            aoes.Add(SimpleElement.Circle(info, 18f));
            break;
        case 40941:
            aoes.Add(SimpleElement.Circle(info, 27f));
            break;
        case 40942:
            aoes.Add(SimpleElement.Donut(info, 9f, 60f));
            break;
        case 40943:
            aoes.Add(SimpleElement.Donut(info, 18f, 60f));
            break;
        case 40944:
            aoes.Add(SimpleElement.Donut(info, 27f, 60f));
            break;
        }
        aoes.SortBy((StaticVfx x) => x.DrawTime);
    }

    public override void OnAbilityCast(ActorAbilityInfo info)
    {
        if (aoes.Count > 0)
        {
            aoes.RemoveAt(0);
        }
    }
}
