using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.TenderValley;

public class SnakeCircle : ISpecialAction
{
    public override string Name => "Snake (circle)";

    public override HashSet<uint> ActionID => new HashSet<uint> { 36749u };

    public override uint Phase => 3u;

    public override void OnActionCast(ActorCastInfo info)
    {
        SimpleElement.Circle(info, 9f);
    }
}
