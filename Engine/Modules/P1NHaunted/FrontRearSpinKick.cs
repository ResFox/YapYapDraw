using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;
using YapYapDraw.Engine.Util;

namespace YapYapDraw.Modules.P1NHaunted;

public class FrontRearSpinKick : ISpecialAction
{
    public override string Name => "Front/Rear Spin Kick";

    public override HashSet<uint> ActionID => new HashSet<uint> { 33095u, 33096u, 33097u, 33098u };

    public override void OnActionCast(ActorCastInfo info)
    {
        switch (info.ActionId)
        {
        case 33095:
        case 33096:
            SimpleElement.Fan(info.SourceId, 40f, 180, info.Facing, 3000f, 0f, 33069u);
            break;
        case 33097:
        case 33098:
            SimpleElement.Fan(info.SourceId, 40f, 180, info.Facing + 180.Degrees(), 3000f, 0f, 33070u);
            break;
        }
    }
}
