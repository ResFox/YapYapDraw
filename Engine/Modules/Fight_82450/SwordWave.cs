using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.Fight_82450;

public class SwordWave : ISpecialAction
{
    public override string Name => "Sword Wave";

    public override HashSet<uint> ActionID => new HashSet<uint> { 35974u };

    public override uint Phase => 1u;

    public override void OnActionCast(ActorCastInfo info)
    {
        SimpleElement.Rectangle(info, 100f, 14f);
    }
}
