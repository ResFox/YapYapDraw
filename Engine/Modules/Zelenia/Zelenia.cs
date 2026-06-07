using YapYapDraw.Engine.ModuleSetup;

namespace YapYapDraw.Modules.Zelenia;

public class Zelenia : DrawModule
{
    public override ModuleInfo ModuleInfo => new ModuleInfo
    {
        Category = Category.Extreme,
        GroupType = GroupType.CFC,
        GroupID = 1031u
    };

    public override string Author => "Res";
}
