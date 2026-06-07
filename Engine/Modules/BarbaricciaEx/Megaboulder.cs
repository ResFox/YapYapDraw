using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.BarbaricciaEx;
public class Megaboulder : ISpecialAction
{
    public override string Name => "Megaboulder";

    public override HashSet<uint> ActionID => new HashSet<uint> { 30107u };

    public override void OnActionCast(ActorCastInfo info)
    {
        SimpleElement.Circle(info.TargetId, 20f, 3000f, 0f, 30107u);
    }
}
