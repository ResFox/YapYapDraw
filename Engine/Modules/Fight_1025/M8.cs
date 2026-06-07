using YapYapDraw.Engine.ModuleSetup;

namespace YapYapDraw.Modules.Fight_1025;

public class M8 : DrawModule
{
    public override ModuleInfo ModuleInfo => new ModuleInfo
    {
        Category = Category.Raid,
        GroupType = GroupType.CFC,
        GroupID = 1025u
    };

    public override bool UseAutoDraw => true;
}
