using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;
using YapYapDraw.Engine.Util;

namespace YapYapDraw.Modules.M5S;

public class EighthBeats : ISpecialAction
{
    public override string Name => "Eighth Beats";

    public override HashSet<uint> ActionID => new HashSet<uint> { 42846u };

    public override void OnActionCast(ActorCastInfo info)
    {
        SimpleLockon.TarLockOn5m5s(info.TargetId.GameObject());
    }
}
