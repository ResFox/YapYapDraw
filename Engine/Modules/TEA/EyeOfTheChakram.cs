using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.TEA;

public class EyeOfTheChakram : ISpecialAction
{
    public override string Name => "Eye of the Chakram";

    public override uint Phase => 2u;

    public override HashSet<uint> ActionID => new HashSet<uint> { 18517u };

    public override void OnActionCast(ActorCastInfo info)
    {
        SimpleElement.Rectangle(info, 73f, 3f, 3f);
    }
}
