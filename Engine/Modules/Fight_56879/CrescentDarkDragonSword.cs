using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.Fight_56879;

public class CrescentDarkDragonSword : ISpecialAction
{
    public override string Name => "Crescent Dark Dragon Sword";

    public override HashSet<uint> ActionID => new HashSet<uint> { 33927u };

    public override uint Phase => 2u;

    public override void OnActionCast(ActorCastInfo info)
    {
        SimpleElement.Fan(info, 22f, 180);
    }
}
