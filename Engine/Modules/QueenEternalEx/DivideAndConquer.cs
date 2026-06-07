using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.QueenEternalEx;
public class DivideAndConquer : ISpecialAction
{
    public override string Name => "Divide and Conquer";

    public override HashSet<uint> ActionID => new HashSet<uint> { 30505u };

    public override void OnActionCast(ActorCastInfo info)
    {
        SimpleElement.Rectangle(info);
    }
}
