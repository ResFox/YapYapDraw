using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.JeunoArc1;

public class EightfoldKick : ISpecialAction
{
    public override string Name => "Eightfold Kick";

    public override HashSet<uint> ActionID => new HashSet<uint> { 40957u };

    public override void OnActionCast(ActorCastInfo info)
    {
        SimpleElement.Circle(info.TargetId, 6f, 3000f, 0f, 40957u);
    }
}
