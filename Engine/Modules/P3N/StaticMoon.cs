using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.P3N;

public class StaticMoon : ISpecialAction
{
    public override string Name => "Static Moon";

    public override HashSet<uint> ActionID => new HashSet<uint> { 30722u };

    public override void OnActionCast(ActorCastInfo info)
    {
        SimpleElement.Circle(info);
    }
}
