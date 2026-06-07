using YapYapDraw.Engine.ModuleSetup;

namespace YapYapDraw.Modules.Fight_15;

public class CastrumMeridianum : DrawModule
{
    public override ModuleInfo ModuleInfo => new ModuleInfo
    {
        Category = Category.Dungeon,
        GroupType = GroupType.CFC,
        GroupID = 15u
    };
}
