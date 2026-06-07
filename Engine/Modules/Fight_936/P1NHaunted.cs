using YapYapDraw.Engine.ModuleSetup;

namespace YapYapDraw.Modules.Fight_936;

public class P1NHaunted : DrawModule
{
    public override ModuleInfo ModuleInfo => new ModuleInfo
    {
        Category = Category.Raid,
        GroupType = GroupType.CFC,
        GroupID = 936u
    };
}
