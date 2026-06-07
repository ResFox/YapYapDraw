using YapYapDraw.Engine.ModuleSetup;

namespace YapYapDraw.Modules.Fight_800;

public class P4N : DrawModule
{
    public override ModuleInfo ModuleInfo => new ModuleInfo
    {
        Category = Category.Raid,
        GroupType = GroupType.CFC,
        GroupID = 800u
    };
}
