using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.AlexandriaDt;

public class AerialAmbush : ISpecialAction
{
    public override string Name => "Aerial Ambush";

    public override HashSet<uint> ActionID => new HashSet<uint> { 42543u };

    public override void OnActionCast(ActorCastInfo info)
    {
        SimpleElement.Rectangle(info.Pos, 30f, 7.5f, 0f, info.Facing, 3000f, 0f, new HitCounter
        {
            ActionID = ActionID
        });
    }
}
