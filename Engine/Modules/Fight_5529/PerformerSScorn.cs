using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.Fight_5529;

public class PerformerSScorn : ISpecialAction
{
    public override string Name => "Performer's Scorn";

    public override HashSet<uint> ActionID => new HashSet<uint> { 26070u };

    public override void OnActionCast(ActorCastInfo info)
    {
        SimpleElement.Circle(info.TargetId, 4f, 3000f, 0f, 26070u);
    }
}
