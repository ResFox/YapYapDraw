using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.CE110;

public class Blowout : ISpecialAction
{
    public override string Name => "Blowout";

    public override HashSet<uint> ActionID => new HashSet<uint> { 41397u };

    public override void OnActionCast(ActorCastInfo info)
    {
        SimpleElement.KnockBack(info, 50f);
    }
}
