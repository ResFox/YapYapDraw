using System.Collections.Generic;
using YapYapDraw.Engine.ModuleSetup;

namespace YapYapDraw.Modules.Fight_1043;

public class Fight_1043 : DrawModule
{
    public override ModuleInfo ModuleInfo => new ModuleInfo
    {
        Category = Category.Trial,
        GroupType = GroupType.CFC,
        GroupID = 1043u
    };

    public override HashSet<uint> NoLogActionID => new HashSet<uint> { 43341u, 43827u, 45175u };
}
