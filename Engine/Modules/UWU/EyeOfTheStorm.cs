using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.UWU;

public class EyeOfTheStorm : ISpecialAction
{
    public override string Name => "Eye of the Typhoon";

    public override uint Phase => 1u;

    public override HashSet<uint> ActionID => new HashSet<uint> { 11090u };

    public override void OnActionCast(ActorCastInfo info)
    {
        SimpleElement.Donut(info, 12f, 25f);
    }
}
