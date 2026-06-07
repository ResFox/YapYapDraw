using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.P1NHaunted;

public class Blaze : ISpecialAction
{
    public override string Name => "Blaze";

    public override HashSet<uint> ActionID => new HashSet<uint> { 33056u };

    public override void OnActionCast(ActorCastInfo info)
    {
        SimpleElement.Circle(info.TargetId, 12f, 3000f, 0f, 33056u);
    }
}
