using YapYapDraw.Engine.ModuleSetup;

namespace YapYapDraw.Modules.Fight_944;

public class Fight_944 : DrawModule
{
    public override ModuleInfo ModuleInfo => new ModuleInfo
    {
        Category = Category.Trial,
        GroupType = GroupType.CFC,
        GroupID = 944u
    };
}
