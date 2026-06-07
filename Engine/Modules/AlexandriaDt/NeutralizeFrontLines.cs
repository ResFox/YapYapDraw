using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.AlexandriaDt;

public class NeutralizeFrontLines : ISpecialAction
{
    public override string Name => "Neutralize Front Lines";

    public override HashSet<uint> ActionID => new HashSet<uint> { 42738u };

    public override void OnActionCast(ActorCastInfo info)
    {
        SimpleElement.Fan(info.Pos, 30f, 180, info.Facing, 3000f, 0f, new HitCounter
        {
            ActionID = ActionID
        });
    }
}
