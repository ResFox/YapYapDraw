using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.DSR;

public class CauterizeN : ISpecialAction
{
    public override string Name => "Cauterize";

    public override uint Phase => 6u;

    public override HashSet<uint> ActionID => new HashSet<uint> { 27966u };

    public override void OnActionCast(ActorCastInfo info)
    {
        SimpleElement.Rectangle(info, 80f, 11f);
    }
}
