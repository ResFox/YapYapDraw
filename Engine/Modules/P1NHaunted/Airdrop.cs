using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.P1NHaunted;

public class Airdrop : ISpecialAction
{
    public override string Name => "Airdrop";

    public override HashSet<uint> ActionID => new HashSet<uint> { 33094u };

    public override void OnActionCast(ActorCastInfo info)
    {
        SimpleElement.Circle(info.TargetId, 14f, 3000f, 0f, 33094u);
    }
}
