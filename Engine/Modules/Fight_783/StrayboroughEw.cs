using System.Collections.Generic;
using YapYapDraw.Engine.ModuleSetup;

namespace YapYapDraw.Modules.Fight_783;

public class StrayboroughEw : DrawModule
{
    public override ModuleInfo ModuleInfo => new ModuleInfo
    {
        Category = Category.Dungeon,
        GroupType = GroupType.CFC,
        GroupID = 783u
    };

    public override HashSet<uint> NoLogActionID => new HashSet<uint> { 870u, 871u, 25241u, 25254u };
}
