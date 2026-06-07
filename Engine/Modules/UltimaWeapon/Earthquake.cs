using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.UltimaWeapon;

public class Earthquake : ISpecialAction
{
    public override string Name => "Earthquake";

    public override HashSet<uint> ActionID => new HashSet<uint> { 29000u, 28981u };

    public override void OnActionCast(ActorCastInfo info)
    {
        SimpleElement.Rectangle(info);
    }
}
