using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.Fight_33798;

public class LockbladeDive : ISpecialAction
{
    public override string Name => "Lockblade Dive";

    public override HashSet<uint> ActionID => new HashSet<uint> { 43835u, 43838u, 43900u, 43903u, 45069u, 45071u, 45099u, 45101u };

    public override void OnActionCast(ActorCastInfo info)
    {
        aoes.Add(SimpleElement.Rectangle(info));
    }

    public override void OnAbilityCast(ActorAbilityInfo info)
    {
        if (aoes.Count > 0)
        {
            aoes.RemoveAt(0);
        }
    }
}
