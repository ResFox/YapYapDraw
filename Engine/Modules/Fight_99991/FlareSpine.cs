using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.Fight_99991;

public class FlareSpine : ISpecialAction
{
    public override string Name => "Flare Spine";

    public override HashSet<uint> ActionID => new HashSet<uint> { 35606u };

    public override void OnActionCast(ActorCastInfo info)
    {
        SimpleElement.Rectangle(info, 60f, 5f);
    }
}
