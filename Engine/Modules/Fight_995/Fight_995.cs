using YapYapDraw.Engine.ModuleSetup;

namespace YapYapDraw.Modules.Fight_995;

public class Fight_995 : DrawModule
{
    public override ModuleInfo ModuleInfo => new ModuleInfo
    {
        Category = Category.Trial,
        GroupType = GroupType.CFC,
        GroupID = 995u
    };
}
