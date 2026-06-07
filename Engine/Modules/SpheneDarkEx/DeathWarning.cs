using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.SpheneDarkEx;
public class DeathWarning : ISpecialAction
{
    public override string Name => "Death Warning";

    public override HashSet<uint> ActionID => new HashSet<uint> { 44565u, 44566u };

    public override void OnActionCast(ActorCastInfo info)
    {
        SimpleElement.Rectangle(info, 100f, 6f);
    }
}
