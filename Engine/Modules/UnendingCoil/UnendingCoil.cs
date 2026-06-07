using YapYapDraw.Engine.ModuleSetup;

namespace YapYapDraw.Modules.UnendingCoil;

public class UnendingCoil : DrawModule
{
    public override ModuleInfo ModuleInfo => new ModuleInfo
    {
        Category = Category.Ultimate,
        GroupType = GroupType.CFC,
        GroupID = 280u
    };

    public override string Author => "Res";
}
