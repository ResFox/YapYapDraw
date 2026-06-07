using System.Collections.Generic;
using YapYapDraw.Engine.ModuleSetup;

namespace YapYapDraw.Modules.M5S;

public class DancingGreen : DrawModule
{
    public override ModuleInfo ModuleInfo => new ModuleInfo
    {
        Category = Category.Savage,
        GroupType = GroupType.CFC,
        GroupID = 1020u
    };

    public override string Author => "Res";

    public override HashSet<(uint Old, uint New)> NoResetPairs => new HashSet<(uint, uint)>
    {
        (2u, 105u),
        (105u, 2u)
    };
}
