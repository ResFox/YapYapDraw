using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.DSR;

public class FlameBlast : ISpecialAction
{
    public override string Name => "Flame Blast";

    public override uint Phase => 6u;

    public override HashSet<uint> ActionID => new HashSet<uint> { 26409u };

    public override void OnActionCast(ActorCastInfo info)
    {
        SimpleElement.Cross(info.SourceId, 44f, 3f, info.Facing, 5000f, 0f, 0u);
    }
}
