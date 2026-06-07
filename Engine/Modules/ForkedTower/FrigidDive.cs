using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.ForkedTower;

public class FrigidDive : ISpecialAction
{
    public override string Name => "Frigid Dive";

    public override HashSet<uint> ActionID => new HashSet<uint> { 37819u };

    public override uint Phase => 3u;

    public override void OnActionCast(ActorCastInfo info)
    {
        SimpleElement.Rectangle(info);
    }
}
