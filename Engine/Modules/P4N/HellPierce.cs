using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.P4N;

public class HellPierce : ISpecialAction
{
    public override string Name => "Hell Pierce";

    public override HashSet<uint> ActionID => new HashSet<uint> { 27215u };

    public override void OnActionCast(ActorCastInfo info)
    {
        SimpleElement.Rectangle(info);
    }
}
