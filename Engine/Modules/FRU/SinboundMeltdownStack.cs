using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;
using YapYapDraw.Engine.Util;

namespace YapYapDraw.Modules.FRU;

public class SinboundMeltdownStack : ISpecialAction
{
    public override string Name => "Sinbound Meltdown (stack)";

    public override uint Phase => 3u;

    public override HashSet<uint> ActionID => new HashSet<uint> { 40286u };

    public override void OnActionCast(ActorCastInfo info)
    {
        SimpleLockon.ShareLockon(info.SourceId.GameObject());
    }
}
