using System.Collections.Generic;
using YapYapDraw.Engine.ModuleSetup;

namespace YapYapDraw.Modules.FRU;

public class FRU : DrawModule
{
    public override ModuleInfo ModuleInfo => new ModuleInfo
    {
        Category = Category.Ultimate,
        GroupType = GroupType.CFC,
        GroupID = 1006u
    };

    public override string Author => "Res";

    public override HashSet<(uint Old, uint New)> NoResetPairs => new HashSet<(uint, uint)>
    {
        (2u, 4u),
        (4u, 2u)
    };
}
