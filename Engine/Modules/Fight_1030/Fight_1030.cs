using YapYapDraw.Engine.ModuleSetup;

namespace YapYapDraw.Modules.Fight_1030;

public class Fight_1030 : DrawModule
{
    public override ModuleInfo ModuleInfo => new ModuleInfo
    {
        Category = Category.Trial,
        GroupType = GroupType.CFC,
        GroupID = 1030u
    };
}
