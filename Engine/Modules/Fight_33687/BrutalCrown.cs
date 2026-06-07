using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.Fight_33687;

public class BrutalCrown : ISpecialAction
{
    public override string Name => "Brutal Crown";

    public override HashSet<uint> ActionID => new HashSet<uint> { 36633u };

    public override void OnActionCast(ActorCastInfo info)
    {
        SimpleElement.Donut(info);
    }
}
