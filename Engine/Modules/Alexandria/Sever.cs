using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.Alexandria;

public class Sever : ISpecialAction
{
    public override string Name => "Sever";

    public override uint Phase => 3u;

    public override HashSet<uint> ActionID => new HashSet<uint> { 39007u, 39238u, 39249u };

    public override void OnActionCast(ActorCastInfo info)
    {
        SimpleElement.Fan(info, 40f, 180);
    }
}
