using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.DiamondWeapon;

public class AetherCannon : ISpecialAction
{
    public override string Name => "Aether Cannon";

    public override HashSet<uint> ActionID => new HashSet<uint> { 24533u };

    public override void OnActionCast(ActorCastInfo info)
    {
        SimpleElement.Rectangle(info);
    }
}
