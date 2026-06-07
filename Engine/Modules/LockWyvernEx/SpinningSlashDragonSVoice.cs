using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.LockWyvernEx;
public class SpinningSlashDragonSVoice : ISpecialAction
{
    public override string Name => "Spinning Slash (Dragon's Voice)";

    public override HashSet<uint> ActionID => new HashSet<uint> { 43916u, 43918u, 45107u, 45108u };

    public override void OnActionCast(ActorCastInfo info)
    {
        SimpleElement.Fan(info);
    }
}
