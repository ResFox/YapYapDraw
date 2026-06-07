using System.Collections.Generic;
using YapYapDraw.Engine.ModuleSetup;

namespace YapYapDraw.Modules.Fight_802;

public class Fight_802 : DrawModule
{
    public override ModuleInfo ModuleInfo => new ModuleInfo
    {
        Category = Category.Trial,
        GroupType = GroupType.CFC,
        GroupID = 802u
    };

    public override HashSet<(uint Old, uint New)> NoResetPairs => new HashSet<(uint, uint)> { (153u, 151u) };

    public override bool UseAutoDraw => true;
}
