using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.P4NHaunted;

public class Glaukopis : ISpecialAction
{
    public override string Name => "Glaukopis";

    public override HashSet<uint> ActionID => new HashSet<uint> { 33493u };

    public override void OnActionCast(ActorCastInfo info)
    {
        SimpleElement.RectangleToTarget(info, 60f, 2.5f);
    }
}
