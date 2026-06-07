using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.Origenics;

public class TelekinesisReact : ISpecialAction
{
    public override string Name => "Telekinesis React";

    public override uint Phase => 3u;

    public override HashSet<uint> ActionID => new HashSet<uint> { 36428u };

    public override void OnActionCast(ActorCastInfo info)
    {
        SimpleElement.Rectangle(info, 70f, 6.5f);
    }
}
