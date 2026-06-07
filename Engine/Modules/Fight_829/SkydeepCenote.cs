using YapYapDraw.Engine.ModuleSetup;

namespace YapYapDraw.Modules.Fight_829;

public class SkydeepCenote : DrawModule
{
    public override ModuleInfo ModuleInfo => new ModuleInfo
    {
        Category = Category.Dungeon,
        GroupType = GroupType.CFC,
        GroupID = 829u
    };
}
