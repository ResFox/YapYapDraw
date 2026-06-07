using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.UltimaWeapon;

public class EarthSmash : ISpecialAction
{
    public override string Name => "Earth Smash";

    public override HashSet<uint> ActionID => new HashSet<uint> { 28999u };

    public override void OnActionCast(ActorCastInfo info)
    {
        SimpleElement.Circle(info, 25f);
    }
}
