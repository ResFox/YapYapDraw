using YapYapDraw.Engine.ModuleSetup;

namespace YapYapDraw.Modules.Fight_781;

public class DiamondWeapon : DrawModule
{
    public override ModuleInfo ModuleInfo => new ModuleInfo
    {
        Category = Category.Trial,
        GroupType = GroupType.CFC,
        GroupID = 781u
    };
}
