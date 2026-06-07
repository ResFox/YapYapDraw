using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.P1NHaunted;

public class StardustImpact : ISpecialAction
{
    public override string Name => "Stardust Impact";

    public override HashSet<uint> ActionID => new HashSet<uint> { 33077u };

    public override void OnActionCast(ActorCastInfo info)
    {
        SimpleElement.Circle(info.TargetId, 15f, 3000f, 0f, 33077u);
    }
}
