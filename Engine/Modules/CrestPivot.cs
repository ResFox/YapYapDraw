using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.M10S;

public class CrestPivot : ISpecialAction
{
    public override string Name => "Crest Pivot";

    public override HashSet<uint> ActionID => new HashSet<uint> { 46547u };

    public override void OnActionCast(ActorCastInfo info)
    {
        SimpleElement.Fan(info);
    }
}
