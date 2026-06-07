using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.Zeromus;

public class MiasmaBurst : ISpecialAction
{
    public override string Name => "Miasma Burst";

    public override uint Phase => 1u;

    public override HashSet<uint> ActionID => new HashSet<uint> { 35657u };

    public override void OnActionCast(ActorCastInfo info)
    {
        SimpleElement.Cross(info);
    }
}
