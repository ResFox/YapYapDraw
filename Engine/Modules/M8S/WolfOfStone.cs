using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.M8S;

public class WolfOfStone : ISpecialAction
{
    public override string Name => "Wolf Of Stone";

    public override HashSet<uint> ActionID => new HashSet<uint> { 42894u, 42898u };

    public override void OnActionCast(ActorCastInfo info)
    {
        SimpleElement.Rectangle(info);
    }
}
