using System.Collections.Generic;
using System.Linq;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;
using YapYapDraw.Engine.Vfx;

namespace YapYapDraw.Modules.Fight_63529;

public class AbyssalEchoes : ISpecialAction
{
    public override string Name => "Abyssal Echoes";

    public override uint Phase => 1u;

    public override HashSet<uint> ActionID => new HashSet<uint> { 35650u };

    public override IEnumerable<StaticVfx> ActiveAOEs => aoes.Take(5);

    public override void OnActionCast(ActorCastInfo info)
    {
        aoes.Add(SimpleElement.Circle(info.TargetId, 12f, 16000f, 0f, 0u, null));
    }

    public override void OnAbilityCast(ActorAbilityInfo info)
    {
        if (aoes.Count > 0)
        {
            aoes.RemoveAt(0);
        }
    }
}
