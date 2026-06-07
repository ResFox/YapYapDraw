using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.Fight_33798;

public class DragonSVoiceCircle : ISpecialAction
{
    public override string Name => "Dragon's Voice (circle)";

    public override HashSet<uint> ActionID => new HashSet<uint> { 43860u };

    public override void OnActionCast(ActorCastInfo info)
    {
        SimpleElement.LineCircle(info, 8f, 1500f, 3);
    }
}
