using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.CloudOfDarkness;

public class GhastlyGloomCross : ISpecialAction
{
    public override string Name => "Ghastly Gloom (cross)";

    public override HashSet<uint> ActionID => new HashSet<uint> { 40458u };

    public override uint Phase => 3u;

    public override void OnActionCast(ActorCastInfo info)
    {
        SimpleElement.Cross(info, 40f, 15f);
    }
}
