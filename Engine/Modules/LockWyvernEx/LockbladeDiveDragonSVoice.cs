using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.LockWyvernEx;
public class LockbladeDiveDragonSVoice : ISpecialAction
{
    public override string Name => "Lockblade Dive (Dragon's Voice)";

    public override HashSet<uint> ActionID => new HashSet<uint> { 43903u, 45101u };

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
