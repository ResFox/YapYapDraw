using YapYapDraw.Engine.ModuleSetup;

namespace YapYapDraw.Modules.Everkeep;

public class Everkeep : DrawModule
{
    public override ModuleInfo ModuleInfo => new ModuleInfo
    {
        Category = Category.Extreme,
        GroupType = GroupType.CFC,
        GroupID = 996u
    };

    public override string Author => "Res";
}
