using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.A4;

public class Sacrament : ISpecialAction
{
    public override string Name => "Sacrament";

    public override HashSet<uint> ActionID => new HashSet<uint> { 6885u };

    public override void OnActionCast(ActorCastInfo info)
    {
        SimpleElement.Cross(info.SourceId, 60f, 8f, info.Facing, 3000f, 0f, 6886u);
    }
}
