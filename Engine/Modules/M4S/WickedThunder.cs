using System.Collections.Generic;
using YapYapDraw.Engine.ModuleSetup;

namespace YapYapDraw.Modules.M4S;

public class WickedThunder : DrawModule
{
    public override ModuleInfo ModuleInfo => new ModuleInfo
    {
        Category = Category.Savage,
        GroupType = GroupType.CFC,
        GroupID = 992u
    };

    public override string Author => "Res";

    public override HashSet<(uint Old, uint New)> NoResetPairs => new HashSet<(uint, uint)> { (2u, 107u) };
}
