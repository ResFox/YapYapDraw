using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.UWU;

public class P2CrimsonCyclone : ISpecialAction
{
    public override string Name => "Ifrit Crimson Cyclone";

    public override uint Phase => 2u;

    public override HashSet<uint> ActionID => new HashSet<uint> { 11103u };

    public override void OnActionCast(ActorCastInfo info)
    {
        SimpleElement.Rectangle(info, 49f, 9f, 5f);
    }
}
