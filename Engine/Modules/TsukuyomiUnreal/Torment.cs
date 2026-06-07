using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.TsukuyomiUnreal;

public class Torment : ISpecialAction
{
    public override string Name => "Torment";

    public override HashSet<uint> ActionID => new HashSet<uint> { 45359u, 45418u };

    public override void OnActionCast(ActorCastInfo info)
    {
        SimpleElement.FanToTarget(info, 15f, 90);
    }
}
