using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.GolbezEx;
public class LightningGlint : ISpecialAction
{
    public override string Name => "Lightning Glint";

    public override HashSet<uint> ActionID => new HashSet<uint> { 45666u };

    public override void OnActionCast(ActorCastInfo info)
    {
        SimpleElement.Circle(info);
    }
}
