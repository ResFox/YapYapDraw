using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.BarbaricciaEx;
public class BoulderBreak : ISpecialAction
{
    public override string Name => "Boulder Break";

    public override HashSet<uint> ActionID => new HashSet<uint> { 29571u };

    public override void OnActionCast(ActorCastInfo info)
    {
        SimpleElement.Circle(info.TargetId, 6f, 3000f, 0f, 29571u);
    }
}
