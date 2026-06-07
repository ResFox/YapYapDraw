using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.LockWyvernEx;
public class LockbladeDiveResonance : ISpecialAction
{
    public override string Name => "Lockblade Dive (Resonance)";

    public override HashSet<uint> ActionID => new HashSet<uint> { 43900u, 45099u, 43901u, 45100u };

    public override void OnActionCast(ActorCastInfo info)
    {
        SimpleElement.Rectangle(info);
    }
}
