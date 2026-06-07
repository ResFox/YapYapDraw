using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.M12S.Body;

public class WingedCatastrophe : ISpecialAction
{
    public override string Name => "Winged Catastrophe";

    public override uint Phase => 2u;

    public override HashSet<uint> ActionID => new HashSet<uint> { 46300u };

    public override void OnActionCast(ActorCastInfo info)
    {
        SimpleElement.Fan(info);
    }
}
