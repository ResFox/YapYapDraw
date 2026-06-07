using YapYapDraw.Engine.ModuleSetup;

namespace YapYapDraw.Modules.Fight_1008;

public class Fight_1008 : DrawModule
{
    public override ModuleInfo ModuleInfo => new ModuleInfo
    {
        Category = Category.Dungeon,
        GroupType = GroupType.CFC,
        GroupID = 1008u
    };
}
