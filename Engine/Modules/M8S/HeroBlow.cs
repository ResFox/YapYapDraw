using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.M8S;

public class HeroBlow : ISpecialAction
{
    public override string Name => "Hero Blow";

    public override uint Phase => 2u;

    public override HashSet<uint> ActionID => new HashSet<uint> { 42080u, 42082u };

    public override void OnActionCast(ActorCastInfo info)
    {
        SimpleElement.Fan(info);
    }
}
