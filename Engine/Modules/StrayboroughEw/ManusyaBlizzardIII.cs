using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.StrayboroughEw;

public class ManusyaBlizzardIII : ISpecialAction
{
    public override string Name => "Manusya Blizzard III";

    public override HashSet<uint> ActionID => new HashSet<uint> { 25238u };

    public override void OnActionCast(ActorCastInfo info)
    {
        SimpleElement.Fan(info);
    }
}
