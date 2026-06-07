using YapYapDraw.Engine.ModuleSetup;

namespace YapYapDraw.Modules.Fight_876;

public class P3N : DrawModule
{
    public override ModuleInfo ModuleInfo => new ModuleInfo
    {
        Category = Category.Raid,
        GroupType = GroupType.CFC,
        GroupID = 876u
    };
}
