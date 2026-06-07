using YapYapDraw.Engine.ModuleSetup;

namespace YapYapDraw.Modules.EmeraldWeapon;

public class EmeraldWeapon : DrawModule
{
    public override ModuleInfo ModuleInfo => new ModuleInfo
    {
        Category = Category.Trial,
        GroupType = GroupType.CFC,
        GroupID = 762u
    };

    public override bool UseAutoDraw => true;
}
