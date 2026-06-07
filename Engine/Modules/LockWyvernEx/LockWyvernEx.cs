using YapYapDraw.Engine.ModuleSetup;

namespace YapYapDraw.Modules.LockWyvernEx;
public class LockWyvernEx : DrawModule
{
    public override ModuleInfo ModuleInfo => new ModuleInfo
    {
        Category = Category.Extreme,
        GroupType = GroupType.CFC,
        GroupID = 1044u
    };
}
