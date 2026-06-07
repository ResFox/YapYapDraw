using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.Fight_33798;

public class SpinningSlashDragonSVoice : ISpecialAction
{
    public override string Name => "Spinning Slash (Dragon's Voice)";

    public override HashSet<uint> ActionID => new HashSet<uint> { 43849u, 43851u, 45061u, 45063u };

    public override void OnActionCast(ActorCastInfo info)
    {
        SimpleElement.Fan(info);
    }
}
