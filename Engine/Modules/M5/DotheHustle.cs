using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.M5;

public class DotheHustle : ISpecialAction
{
    public override string Name => "Do the Hustle";

    public override HashSet<uint> ActionID => new HashSet<uint> { 42697u, 42698u };

    public override void OnActionCast(ActorCastInfo info)
    {
        SimpleElement.Fan(info.Pos, 50f, 180, info.Facing, 3000f, 0f, new HitCounter
        {
            ActionID = ActionID
        });
    }
}
