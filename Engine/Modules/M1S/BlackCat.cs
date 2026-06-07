using YapYapDraw.Engine.ModuleSetup;

namespace YapYapDraw.Modules.M1S;

public class BlackCat : DrawModule
{
    public override ModuleInfo ModuleInfo => new ModuleInfo
    {
        Category = Category.Savage,
        GroupType = GroupType.CFC,
        GroupID = 986u
    };

    public override string Author => "Res";
}
