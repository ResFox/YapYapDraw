using YapYapDraw.Engine.ModuleSetup;

namespace YapYapDraw.Modules.AloaloVc;

public class AloaloVc : DrawModule
{
    public override ModuleInfo ModuleInfo => new ModuleInfo
    {
        Category = Category.VariantCriterion,
        GroupType = GroupType.CFC,
        GroupID = 979u
    };

    public override string Author => "Res";
}
