using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.M3;

public class MurderousMist : ISpecialAction
{
    public override string Name => "Murderous Mist";

    public override uint Phase => 1u;

    public override HashSet<uint> ActionID => new HashSet<uint> { 37813u };

    public override void OnActionCast(ActorCastInfo info)
    {
        SimpleElement.Fan(info, 60f, 270);
    }
}
