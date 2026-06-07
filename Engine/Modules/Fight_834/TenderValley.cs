using YapYapDraw.Engine.ModuleSetup;

namespace YapYapDraw.Modules.Fight_834;

public class TenderValley : DrawModule
{
    public override ModuleInfo ModuleInfo => new ModuleInfo
    {
        Category = Category.Dungeon,
        GroupType = GroupType.CFC,
        GroupID = 834u
    };
}
