using YapYapDraw.Engine.ModuleSetup;

namespace YapYapDraw.Modules.Fight_981;

public class Fight_981 : DrawModule
{
    public override ModuleInfo ModuleInfo => new ModuleInfo
    {
        Category = Category.Dungeon,
        GroupType = GroupType.CFC,
        GroupID = 981u
    };
}
