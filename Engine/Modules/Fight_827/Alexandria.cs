using YapYapDraw.Engine.ModuleSetup;

namespace YapYapDraw.Modules.Fight_827;

public class Alexandria : DrawModule
{
    public override ModuleInfo ModuleInfo => new ModuleInfo
    {
        Category = Category.Dungeon,
        GroupType = GroupType.CFC,
        GroupID = 827u
    };
}
