using YapYapDraw.Engine.ModuleSetup;

namespace YapYapDraw.Modules.theGreatHunt;

public class theGreatHunt : DrawModule
{
    public override ModuleInfo ModuleInfo => new ModuleInfo
    {
        Category = Category.Trial,
        GroupType = GroupType.CFC,
        GroupID = 474u
    };
}
