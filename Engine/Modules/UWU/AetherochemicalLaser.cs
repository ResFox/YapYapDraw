using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.UWU;

public class AetherochemicalLaser : ISpecialAction
{
    public override string Name => "P3 Aetherochemical Laser";

    public override uint Phase => 4u;

    public override HashSet<uint> ActionID => new HashSet<uint> { 11140u, 11141u, 11142u };

    public override void OnActionCast(ActorCastInfo info)
    {
        SimpleElement.Rectangle(info, 46f, 4f, 6f);
    }
}
