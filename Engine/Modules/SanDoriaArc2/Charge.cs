using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.SanDoriaArc2;

public class Charge : ISpecialAction
{
    public override string Name => "Charge";

    public override HashSet<uint> ActionID => new HashSet<uint> { 44295u };

    public override void OnActionCast(ActorCastInfo info)
    {
        SimpleElement.Rectangle(info);
    }
}
