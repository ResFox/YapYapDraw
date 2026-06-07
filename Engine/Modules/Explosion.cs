using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.M11S;

public class Explosion : ISpecialAction
{
    public override string Name => "Explosion";

    public override HashSet<uint> ActionID => new HashSet<uint> { 46112u };

    public override void OnActionCast(ActorCastInfo info)
    {
        SimpleElement.Rectangle(info);
    }
}
