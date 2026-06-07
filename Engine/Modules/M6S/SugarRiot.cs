using System.Collections.Generic;
using YapYapDraw.Engine.ModuleSetup;

namespace YapYapDraw.Modules.M6S;

public class SugarRiot : DrawModule
{
    public override ModuleInfo ModuleInfo => new ModuleInfo
    {
        Category = Category.Savage,
        GroupType = GroupType.CFC,
        GroupID = 1022u
    };

    public override string Author => "Res";

    public override HashSet<(uint Old, uint New)> NoResetPairs => new HashSet<(uint, uint)> { (2u, 105u) };

    public override HashSet<uint> NoLogActionID => new HashSet<uint> { 42932u, 37919u, 37920u, 42611u, 42672u, 42620u };
}
