using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.CenoteJaJa;

public class GoldenSplash : ISpecialAction
{
    public override string Name => "Golden Splash";

    public override HashSet<uint> ActionID => new HashSet<uint> { 38267u };

    public override void OnActionCast(ActorCastInfo info)
    {
        SimpleElement.Fan(info, 180);
    }
}
