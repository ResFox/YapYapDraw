using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.GolbezEx;
public class LightningRay : ISpecialAction
{
    public override string Name => "Lightning Ray";

    public override HashSet<uint> ActionID => new HashSet<uint> { 45681u, 45682u, 45683u, 45686u };

    public override void OnActionCast(ActorCastInfo info)
    {
        SimpleElement.RectangleMdl(info);
    }
}
