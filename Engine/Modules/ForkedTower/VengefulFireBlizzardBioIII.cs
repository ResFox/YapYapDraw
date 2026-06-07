using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.ForkedTower;

public class VengefulFireBlizzardBioIII : ISpecialAction
{
    public override string Name => "Vengeful Fire/Blizzard/Bio III";

    public override HashSet<uint> ActionID => new HashSet<uint> { 42429u, 42430u, 42431u };

    public override uint Phase => 2u;

    public override void OnActionCast(ActorCastInfo info)
    {
        SimpleElement.Fan(info);
    }
}
