using YapYapDraw.Engine.ModuleSetup;

namespace YapYapDraw.Modules.Fight_790;

public class Fight_790 : DrawModule
{
    public override ModuleInfo ModuleInfo => new ModuleInfo
    {
        Category = Category.Trial,
        GroupType = GroupType.CFC,
        GroupID = 790u
    };
}
