using System.Collections.Generic;
using System.Linq;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;
using YapYapDraw.Engine.Vfx;

namespace YapYapDraw.Modules.SpheneDarkEx;
public class AzureRing : ISpecialAction
{
    public override string Name => "Azure Ring";

    public override HashSet<uint> ActionID => new HashSet<uint> { 44600u };

    public override IEnumerable<StaticVfx> ActiveAOEs => aoes.Take(1);

    public override void OnActionCast(ActorCastInfo info)
    {
        aoes.Add(SimpleElement.Donut(info.Pos, 3f, 50f, 7000f));
    }

    public override void OnAbilityCast(ActorAbilityInfo info)
    {
        if (aoes.Count > 0)
        {
            aoes[0].Remove();
            aoes.RemoveAt(0);
        }
    }
}
