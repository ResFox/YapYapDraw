using YapYapDraw.Engine.ModuleSetup;

namespace YapYapDraw.Modules.Fight_1027;

public class AlexandriaDt : DrawModule
{
    public override ModuleInfo ModuleInfo => new ModuleInfo
    {
        Category = Category.Dungeon,
        GroupType = GroupType.CFC,
        GroupID = 1027u
    };
}
