using YapYapDraw.Engine.ModuleSetup;

namespace YapYapDraw.Modules.HoneyBLovely;

public class HoneyBLovely : DrawModule
{
    public override ModuleInfo ModuleInfo => new ModuleInfo
    {
        Category = Category.Raid,
        GroupType = GroupType.CFC,
        GroupID = 987u
    };
}
