using System.Collections.Generic;
using YapYapDraw.Engine.ModuleSetup;

namespace YapYapDraw.Modules.M12S;

public class Lindblum : DrawModule
{
    public override ModuleInfo ModuleInfo => new ModuleInfo
    {
        Category = Category.Savage,
        GroupType = GroupType.CFC,
        GroupID = 1075u
    };

    public override string Author => "Res";

    public override HashSet<(uint Old, uint New)> NoResetPairs => new HashSet<(uint, uint)>
    {
        (106u, 107u),
        (107u, 106u)
    };
}
