using YapYapDraw.Engine.ModuleSetup;

namespace YapYapDraw.Modules.Fight_942;

public class P4NHaunted : DrawModule
{
    public override ModuleInfo ModuleInfo => new ModuleInfo
    {
        Category = Category.Raid,
        GroupType = GroupType.CFC,
        GroupID = 942u
    };
}
