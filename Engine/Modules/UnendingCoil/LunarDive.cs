using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.UnendingCoil;

public class LunarDive : ISpecialAction
{
    public override string Name => "Lunar Dive";

    public override uint Phase => 3u;

    public override HashSet<uint> ActionID => new HashSet<uint> { 9923u };

    public override void OnActionCast(ActorCastInfo info)
    {
        SimpleElement.Rectangle(info, 60f, 4f);
    }
}
