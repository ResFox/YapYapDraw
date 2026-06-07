using YapYapDraw.Engine.ModuleSetup;

namespace YapYapDraw.Modules.Fight_825;

public class Origenics : DrawModule
{
    public override ModuleInfo ModuleInfo => new ModuleInfo
    {
        Category = Category.Dungeon,
        GroupType = GroupType.CFC,
        GroupID = 825u
    };
}
