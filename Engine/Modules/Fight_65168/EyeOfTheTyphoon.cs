using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.Fight_65168;

public class EyeOfTheTyphoon : ISpecialAction
{
    public override string Name => "Eye of the Typhoon";

    public override HashSet<uint> ActionID => new HashSet<uint> { 28980u };

    public override void OnActionCast(ActorCastInfo info)
    {
        SimpleElement.Donut(info, 12.5f, 25f);
    }
}
