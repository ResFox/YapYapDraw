using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.A4S;

public class MegaBeam : ISpecialAction
{
    public override string Name => "Mega Beam";

    public override HashSet<uint> ActionID => new HashSet<uint> { 5938u };

    public override void OnActionCast(ActorCastInfo info)
    {
        SimpleElement.Rectangle(info);
    }
}
