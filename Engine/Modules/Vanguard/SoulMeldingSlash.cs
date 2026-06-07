using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.Vanguard;

public class SoulMeldingSlash : ISpecialAction
{
    public override string Name => "Soul-melding Slash";

    public override uint Phase => 3u;

    public override HashSet<uint> ActionID => new HashSet<uint> { 36580u, 36581u, 36582u, 36583u, 36585u, 36586u, 36587u, 36588u };

    public override void OnActionCast(ActorCastInfo info)
    {
        SimpleElement.Fan(info, 19f, 90);
    }
}
