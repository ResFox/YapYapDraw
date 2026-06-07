using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.DSR;

public class TwistingDive : ISpecialAction
{
    public override string Name => "Twisting Dive";

    public override uint Phase => 5u;

    public override HashSet<uint> ActionID => new HashSet<uint> { 27531u };

    public override void OnActionCast(ActorCastInfo info)
    {
        SimpleElement.Rectangle(info, 60f, 5f);
    }
}
