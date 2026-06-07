using YapYapDraw.Engine.ModuleSetup;

namespace YapYapDraw.Modules.BarbaricciaEx;
public class BarbaricciaEx : DrawModule
{
    public override ModuleInfo ModuleInfo => new ModuleInfo
    {
        Category = Category.Extreme,
        GroupType = GroupType.CFC,
        GroupID = 871u
    };
}
