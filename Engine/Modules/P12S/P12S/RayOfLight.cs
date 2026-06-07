using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.P12S.P12S;

public class RayOfLight : ISpecialAction
{
    public override string Name => "Ray of Light";

    public override uint Phase => 1u;

    public override HashSet<uint> ActionID => new HashSet<uint> { 33518u };

    public override void OnActionCast(ActorCastInfo info)
    {
        SimpleElement.Rectangle(info, 60f, 5f);
    }
}
