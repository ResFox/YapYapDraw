using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;
using YapYapDraw.Engine.Util;

namespace YapYapDraw.Modules.Fight_7396;

public class DragonMawTailSmash : ISpecialAction
{
    public override string Name => "Dragon Maw + Tail Smash";

    public override HashSet<uint> ActionID => new HashSet<uint> { 10346u, 10362u };

    public override void OnActionCast(ActorCastInfo info)
    {
        if (info.ActionId == 10346)
        {
            SimpleElement.Fan(info.Pos, 10f, 90, info.Facing, 3000f, 0f, new HitCounter
            {
                ActionID = new HashSet<uint> { 10346u }
            });
            SimpleElement.Fan(info.Pos, 11f, 90, info.Facing + 135f.Degrees(), 3000f, 0f, new HitCounter
            {
                ActionID = new HashSet<uint> { 10347u }
            });
        }
        if (info.ActionId == 10362)
        {
            SimpleElement.Fan(info.Pos, 9f, 90, info.Facing, 3000f, 0f, new HitCounter
            {
                ActionID = new HashSet<uint> { 10362u }
            });
        }
    }
}
