using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.ForkedTower;

public class DraconiformMotion : ISpecialAction
{
    public override string Name => "Draconiform Motion";

    public override HashSet<uint> ActionID => new HashSet<uint> { 30693u, 30694u };

    public override uint Phase => 3u;

    public override void OnActionCast(ActorCastInfo info)
    {
        SimpleElement.Fan(info);
    }
}
