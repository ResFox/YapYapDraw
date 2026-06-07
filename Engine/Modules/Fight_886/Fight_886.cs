using System.Collections.Generic;
using YapYapDraw.Engine.ModuleSetup;

namespace YapYapDraw.Modules.Fight_886;

public class Fight_886 : DrawModule
{
    public override ModuleInfo ModuleInfo => new ModuleInfo
    {
        Category = Category.Trial,
        GroupType = GroupType.CFC,
        GroupID = 886u
    };

    public override HashSet<(uint Old, uint New)> NoResetPairs => new HashSet<(uint, uint)>
    {
        (2u, 106u),
        (106u, 2u)
    };
}
