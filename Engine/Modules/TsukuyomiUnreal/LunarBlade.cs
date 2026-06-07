using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;
using YapYapDraw.Engine.Util;

namespace YapYapDraw.Modules.TsukuyomiUnreal;

public class LunarBlade : ISpecialAction
{
    public override string Name => "Lunar Blade";

    public override HashSet<uint> ActionID => new HashSet<uint> { 45390u, 45391u };

    public override void OnActionCast(ActorCastInfo info)
    {
        SimpleElement.Fan(info, 210);
        if (info.SourceId.GameObject().HasStatus(1535u))
        {
            SimpleElement.Circle(info.SourceId.GameObject(), 10f, 3000f, 0f, new HitCounter
            {
                ActionID = new HashSet<uint> { 45394u }
            });
        }
        if (info.SourceId.GameObject().HasStatus(1536u))
        {
            SimpleElement.Donut(info.SourceId.GameObject(), 5f, 40f, 3000f, 0f, new HitCounter
            {
                ActionID = new HashSet<uint> { 45395u }
            });
        }
    }
}
