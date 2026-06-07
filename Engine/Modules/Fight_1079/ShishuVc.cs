using System.Collections.Generic;
using YapYapDraw.Engine.ModuleSetup;

namespace YapYapDraw.Modules.Fight_1079;

public class ShishuVc : DrawModule
{
    public override ModuleInfo ModuleInfo => new ModuleInfo
    {
        Category = Category.VariantCriterion,
        GroupType = GroupType.CFC,
        GroupID = 1079u
    };

    public override HashSet<uint> NoLogActionID => new HashSet<uint> { 45838u, 45128u, 45545u };

    public override string Author => "Res";
}
