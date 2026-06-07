using YapYapDraw.Engine.ModuleSetup;

namespace YapYapDraw.Modules.SugarRiot;

public class SugarRiot : DrawModule
{
    public override ModuleInfo ModuleInfo => new ModuleInfo
    {
        Category = Category.Raid,
        GroupType = GroupType.CFC,
        GroupID = 1021u
    };

    public override string Author => "Res";
}
