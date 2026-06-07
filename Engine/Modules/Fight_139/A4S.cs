using YapYapDraw.Engine.ModuleSetup;

namespace YapYapDraw.Modules.Fight_139;

public class A4S : DrawModule
{
    public override ModuleInfo ModuleInfo => new ModuleInfo
    {
        Category = Category.Raid,
        GroupType = GroupType.CFC,
        GroupID = 139u
    };
}
