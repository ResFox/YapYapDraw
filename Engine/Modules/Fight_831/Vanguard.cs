using YapYapDraw.Engine.ModuleSetup;

namespace YapYapDraw.Modules.Fight_831;

public class Vanguard : DrawModule
{
    public override ModuleInfo ModuleInfo => new ModuleInfo
    {
        Category = Category.Dungeon,
        GroupType = GroupType.CFC,
        GroupID = 831u
    };
}
