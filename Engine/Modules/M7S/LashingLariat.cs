using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.M7S;

public class LashingLariat : ISpecialAction
{
    public override string Name => "Lashing Lariat";

    public override uint Phase => 3u;

    public override HashSet<uint> ActionID => new HashSet<uint> { 42408u, 42410u };

    public override void OnActionCast(ActorCastInfo info)
    {
        SimpleElement.Rectangle(info);
    }
}
