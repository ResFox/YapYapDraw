using YapYapDraw.Engine.ModuleSetup;

namespace YapYapDraw.Modules.SpheneDarkEx;
public class SpheneDarkEx : DrawModule
{
    public override ModuleInfo ModuleInfo => new ModuleInfo
    {
        Category = Category.Extreme,
        GroupType = GroupType.CFC,
        GroupID = 1062u
    };
}
