using YapYapDraw.Engine.ModuleSetup;

namespace YapYapDraw.Modules.Fight_830;

public class UltimaWeapon : DrawModule
{
    public override ModuleInfo ModuleInfo => new ModuleInfo
    {
        Category = Category.Trial,
        GroupType = GroupType.CFC,
        GroupID = 830u
    };
}
