using YapYapDraw.Engine.ModuleSetup;

namespace YapYapDraw.Modules.JeunoArc1;

public class JeunoArc1 : DrawModule
{
    public override ModuleInfo ModuleInfo => new ModuleInfo
    {
        Category = Category.Alliance,
        GroupType = GroupType.CFC,
        GroupID = 1015u
    };
}
