using YapYapDraw.Engine.ModuleSetup;

namespace YapYapDraw.Modules.Fight_16;

public class Praetorium : DrawModule
{
    public override ModuleInfo ModuleInfo => new ModuleInfo
    {
        Category = Category.Dungeon,
        GroupType = GroupType.CFC,
        GroupID = 16u
    };
}
