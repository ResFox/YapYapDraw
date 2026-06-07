using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;
using YapYapDraw.Engine.Util;
using YapYapDraw.Engine.Vfx;

namespace YapYapDraw.Modules.DancingMad.P2;

public class AnnihilationKick : ISpecialAction
{
    public override string Name => "Annihilation Kick";

    public override uint Phase => 2u;

    public override HashSet<uint> ActionID => new HashSet<uint> { 47836u, 47837u };

    public override void OnActionCast(ActorCastInfo info)
    {
        List<StaticVfx>? stored = ModuleUtil.GetSpecialAction<PastFutureTermination>()?.aoes;
        if (stored != null && stored.Count > 0)
        {
            stored[0].Remove();
            stored.RemoveAt(0);
        }

        SimpleElement.Fan(info.Pos, 100f, 180, info.Facing, 3000f, 0f, new HitCounter
        {
            ActionID = ActionID
        });
    }
}
