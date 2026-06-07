using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.P12S.P12S;

public class UltimaRay : ISpecialAction
{
    public override string Name => "Ultima Ray";

    public override uint Phase => 2u;

    public override HashSet<uint> ActionID => new HashSet<uint> { 33584u };

    public override void OnActionCast(ActorCastInfo info)
    {
        SimpleElement.Rectangle(info, 20f, 3f);
    }
}
