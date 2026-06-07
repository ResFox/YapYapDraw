using YapYapDraw.Engine.ModuleSetup;

namespace YapYapDraw.Modules.Fight_872;

public class P1N : DrawModule
{
    public override ModuleInfo ModuleInfo => new ModuleInfo
    {
        Category = Category.Raid,
        GroupType = GroupType.CFC,
        GroupID = 872u
    };
}
