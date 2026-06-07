using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.UWU;

public class WeightOfTheLand : ISpecialAction
{
    public override string Name => "Weight of the Land";

    public override uint Phase => 3u;

    public override HashSet<uint> ActionID => new HashSet<uint> { 11109u };

    public override void OnActionCast(ActorCastInfo info)
    {
        SimpleElement.Circle(info.Pos, 6f, 3000f, 0f, new HitCounter
        {
            ActionID = new HashSet<uint> { 11109u }
        });
    }
}
