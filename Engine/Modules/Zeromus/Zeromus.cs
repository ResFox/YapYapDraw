using YapYapDraw.Engine.ModuleSetup;

namespace YapYapDraw.Modules.Zeromus;

public class Zeromus : DrawModule
{
    public override ModuleInfo ModuleInfo => new ModuleInfo
    {
        Category = Category.Extreme,
        GroupType = GroupType.CFC,
        GroupID = 965u
    };
}
