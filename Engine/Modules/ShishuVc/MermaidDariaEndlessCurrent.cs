using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.ShishuVc;

public class MermaidDariaEndlessCurrent : ISpecialAction
{
    public override string Name => "Mermaid Daria Endless Current";

    public override HashSet<uint> ActionID => new HashSet<uint> { 45863u };

    public override void OnActionCast(ActorCastInfo info)
    {
        SimpleElement.LineRectOffset(info, 8f, 2000f, 5, -4f);
    }
}
