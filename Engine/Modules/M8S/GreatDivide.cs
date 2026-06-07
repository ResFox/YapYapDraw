using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.M8S;

public class GreatDivide : ISpecialAction
{
    public override string Name => "Great Divide";

    public override HashSet<uint> ActionID => new HashSet<uint> { 41944u };

    public override void OnActionCast(ActorCastInfo info)
    {
        SimpleElement.RectangleToTarget(info, 60f, 3f);
    }
}
