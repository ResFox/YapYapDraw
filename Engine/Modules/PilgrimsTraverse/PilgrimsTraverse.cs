using System.Collections.Generic;
using YapYapDraw.Engine.ModuleSetup;

namespace YapYapDraw.Modules.PilgrimsTraverse;

public class PilgrimsTraverse : DrawModule
{
    public override ModuleInfo ModuleInfo => new ModuleInfo
    {
        Category = Category.DeepDungeon,
        GroupType = GroupType.CFC,
        GroupID = 1063u
    };

    public override string Author => "Res";

    public override HashSet<uint> NoLogActionID => new HashSet<uint> { 44820u, 44802u, 44814u, 44137u, 44136u, 44135u, 45197u, 45198u };
}
