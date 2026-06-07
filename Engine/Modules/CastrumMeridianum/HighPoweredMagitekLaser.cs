using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.CastrumMeridianum;

public class HighPoweredMagitekLaser : ISpecialAction
{
    public override string Name => "High-powered Magitek Laser";

    public override HashSet<uint> ActionID => new HashSet<uint> { 28773u };

    public override void OnActionCast(ActorCastInfo info)
    {
        SimpleElement.Rectangle(info);
    }
}
