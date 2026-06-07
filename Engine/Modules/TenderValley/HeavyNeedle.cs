using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.TenderValley;

public class HeavyNeedle : ISpecialAction
{
    public override string Name => "Heavy Needle";

    public override HashSet<uint> ActionID => new HashSet<uint> { 37386u };

    public override uint Phase => 1u;

    public override void OnActionCast(ActorCastInfo info)
    {
        SimpleElement.Fan(info, 36f, 50);
    }
}
