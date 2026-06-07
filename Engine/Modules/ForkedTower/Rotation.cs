using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.ForkedTower;

public class Rotation : ISpecialAction
{
    public override string Name => "Rotation";

    public override HashSet<uint> ActionID => new HashSet<uint> { 41731u, 41732u };

    public override void OnActionCast(ActorCastInfo info)
    {
        switch (info.ActionId)
        {
        case 41731:
            SimpleElement.Fan(info.Pos, 37f, 90, info.Facing, 3000f, 0f, new HitCounter
            {
                ActionID = new HashSet<uint> { info.ActionId }
            });
            break;
        case 41732:
            SimpleElement.Rectangle(info.Pos, 33f, 1.5f, 0f, info.Facing, 3000f, 0f, new HitCounter
            {
                ActionID = new HashSet<uint> { info.ActionId }
            });
            break;
        }
    }
}
