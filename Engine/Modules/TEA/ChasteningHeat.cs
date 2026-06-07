using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.TEA;

public class ChasteningHeat : ISpecialAction
{
    public override string Name => "Chastening Heat";

    public override uint Phase => 3u;

    public override HashSet<uint> ActionID => new HashSet<uint> { 19072u };

    public override void OnActionCast(ActorCastInfo info)
    {
        SimpleElement.Circle(info.TargetId, 5f, 3000f, 0f, 19072u);
    }
}
