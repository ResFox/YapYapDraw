using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.LockWyvernEx;
public class TailThrust : ISpecialAction
{
    public override string Name => "Tail Thrust";

    public override HashSet<uint> ActionID => new HashSet<uint> { 44805u, 45109u };

    public override void OnActionCast(ActorCastInfo info)
    {
        SimpleElement.Rectangle(info);
    }
}
