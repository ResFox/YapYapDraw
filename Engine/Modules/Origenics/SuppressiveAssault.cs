using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.Origenics;

public class SuppressiveAssault : ISpecialAction
{
    public override string Name => "Suppressive Assault";

    public override uint Phase => 3u;

    public override HashSet<uint> ActionID => new HashSet<uint> { 39233u, 39072u };

    public override void OnActionCast(ActorCastInfo info)
    {
        SimpleElement.Fan(info, 26f, 180);
    }
}
