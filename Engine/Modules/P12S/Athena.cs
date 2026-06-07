using YapYapDraw.Engine.ModuleSetup;

namespace YapYapDraw.Modules.P12S;

public class Athena : DrawModule
{
    public override ModuleInfo ModuleInfo => new ModuleInfo
    {
        Category = Category.Savage,
        GroupType = GroupType.CFC,
        GroupID = 943u
    };
}
