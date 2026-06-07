using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.Alexandria;

public class Termination : ISpecialAction
{
    public override string Name => "Termination";

    public override uint Phase => 3u;

    public override HashSet<uint> ActionID => new HashSet<uint> { 39615u };

    public override void OnActionCast(ActorCastInfo info)
    {
        SimpleElement.Rectangle(info, 40f, 5f);
    }
}
