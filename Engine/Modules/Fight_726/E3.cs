using YapYapDraw.Engine.ModuleSetup;

namespace YapYapDraw.Modules.Fight_726;

public class E3 : DrawModule
{
    public override ModuleInfo ModuleInfo => new ModuleInfo
    {
        Category = Category.Raid,
        GroupType = GroupType.CFC,
        GroupID = 726u
    };
}
