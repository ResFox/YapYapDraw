using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.UnendingCoil;

public class Exaflare : ISpecialAction
{
    public override string Name => "Exaflare";

    public override uint Phase => 5u;

    public override HashSet<uint> ActionID => new HashSet<uint> { 9968u };

    public override void OnActionCast(ActorCastInfo info)
    {
        SimpleElement.LineCircle(info, 8f, 1500f, 6);
    }
}
