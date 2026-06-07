using System.Collections.Generic;
using YapYapDraw.Engine.ModuleSetup;

namespace YapYapDraw.Modules.M10S;

public class LimitBrothers : DrawModule
{
    public override ModuleInfo ModuleInfo => new ModuleInfo
    {
        Category = Category.Savage,
        GroupType = GroupType.CFC,
        GroupID = 1071u
    };

    public override string Author => "Res";

    public override HashSet<uint> NoLogActionID => new HashSet<uint> { 48639u, 48640u };
}
