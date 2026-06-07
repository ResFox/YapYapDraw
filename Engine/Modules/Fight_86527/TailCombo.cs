using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.Fight_86527;

public class TailCombo : ISpecialAction
{
    public override string Name => "Tail Combo";

    public override HashSet<uint> ActionID => new HashSet<uint> { 44161u };

    public override void OnActionCast(ActorCastInfo info)
    {
        SimpleElement.Rectangle(info);
    }
}
