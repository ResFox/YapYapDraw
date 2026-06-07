using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.Fight_56879;

public class CrescentSword : ISpecialAction
{
    public override string Name => "Crescent Sword";

    public override HashSet<uint> ActionID => new HashSet<uint> { 33899u };

    public override uint Phase => 1u;

    public override void OnActionCast(ActorCastInfo info)
    {
        SimpleElement.Fan(info, 22f, 180);
    }
}
