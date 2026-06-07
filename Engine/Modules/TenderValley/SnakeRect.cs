using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.TenderValley;

public class SnakeRect : ISpecialAction
{
    public override string Name => "Snake (rect)";

    public override HashSet<uint> ActionID => new HashSet<uint> { 36750u };

    public override uint Phase => 3u;

    public override void OnActionCast(ActorCastInfo info)
    {
        SimpleElement.Rectangle(info, 25.5f, 2.5f, 25.5f);
    }
}
