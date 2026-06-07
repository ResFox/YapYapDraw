using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.Fight_65168;

public class MagitekLaser : ISpecialAction
{
    public override string Name => "Magitek Laser";

    public override HashSet<uint> ActionID => new HashSet<uint> { 29008u, 29009u, 29010u };

    public override void OnActionCast(ActorCastInfo info)
    {
        SimpleElement.Rectangle(info);
    }
}
