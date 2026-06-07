using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.M11S;

public class BeastFlameTail : ISpecialAction
{
    public override string Name => "Beast Flame Tail";

    public override HashSet<uint> ActionID => new HashSet<uint> { 46128u, 46129u };

    public override void OnActionCast(ActorCastInfo info)
    {
        SimpleElement.Fan(info, 90);
    }
}
