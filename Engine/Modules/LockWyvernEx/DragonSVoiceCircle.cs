using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.LockWyvernEx;
public class DragonSVoiceCircle : ISpecialAction
{
    public override string Name => "Dragon's Voice (circle)";

    public override HashSet<uint> ActionID => new HashSet<uint> { 43926u, 43952u };

    public override void OnActionCast(ActorCastInfo info)
    {
        SimpleElement.LineCircle(info, 8f, 1100f, (info.ActionId == 43926) ? 2 : 5);
    }
}
