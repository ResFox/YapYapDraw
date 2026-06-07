using YapYapDraw.Engine.ModuleSetup;

namespace YapYapDraw.Modules.M3S;

public class BruteBomber : DrawModule
{
    public override ModuleInfo ModuleInfo => new ModuleInfo
    {
        Category = Category.Savage,
        GroupType = GroupType.CFC,
        GroupID = 990u
    };

    public override string Author => "Res";
}
