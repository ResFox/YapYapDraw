using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;
using YapYapDraw.Engine.Util;

namespace YapYapDraw.Modules.M6S;

public class PuddingGraf : ISpecialAction
{
    public override string Name => "Pudding Graf";

    public override uint Phase => 2u;

    public override HashSet<uint> ActionID => new HashSet<uint> { 42678u };

    public override void OnActionCast(ActorCastInfo info)
    {
        SimpleLockon.TarLockOn6m5s(info.TargetId.GameObject());
    }
}
