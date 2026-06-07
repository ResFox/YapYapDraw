using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.Fight_86527;

public class RadiantSword : ISpecialAction
{
    public override string Name => "Radiant Sword";

    public override HashSet<uint> ActionID => new HashSet<uint> { 44104u, 44110u };

    public override void OnActionCast(ActorCastInfo info)
    {
        SimpleElement.Rectangle(info);
    }
}
