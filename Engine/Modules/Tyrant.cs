using System.Collections.Generic;
using YapYapDraw.Engine.ModuleSetup;

namespace YapYapDraw.Modules.M11S;

public class Tyrant : DrawModule
{
    public override ModuleInfo ModuleInfo => new ModuleInfo
    {
        Category = Category.Savage,
        GroupType = GroupType.CFC,
        GroupID = 1073u
    };

    public override string Author => "Res";

    public override HashSet<uint> NoLogActionID => new HashSet<uint> { 46085u };
}
