using System.Collections.Generic;
using YapYapDraw.Engine.ModuleSetup;

namespace YapYapDraw.Modules.DancingMad;

public class DancingMad : DrawModule
{
    public override ModuleInfo ModuleInfo => new ModuleInfo
    {
        Category = Category.Ultimate,
        GroupType = GroupType.CFC,
        GroupID = 1094u
    };

    public override string Author => "Res";

    public override HashSet<uint> NoLogActionID => new HashSet<uint> { 49746u, 49744u };

    public override HashSet<(uint Old, uint New)> NoResetPairs => new HashSet<(uint, uint)> { (77u, 78u) };

    public override Dictionary<uint, HashSet<uint>> BlockOmenMap { get; } = new Dictionary<uint, HashSet<uint>>
    {
        [77u] = new HashSet<uint> { 47768u, 47771u, 47774u, 47775u, 47776u, 47777u },
        [78u] = new HashSet<uint> { 47768u, 47771u, 47774u, 47775u, 47776u, 47777u },
        [174u] = new HashSet<uint> { 47768u, 47771u, 47774u, 47775u, 47776u, 47777u }
    };

    public override Dictionary<uint, HashSet<string>> BlockOmenPathMap
    {
        get
        {
            Dictionary<uint, HashSet<string>> map = new Dictionary<uint, HashSet<string>>
            {
                [77u] = new HashSet<string> { "vfx/lockon/eff/m0462trg_a0c.avfx", "vfx/lockon/eff/m0462trg_b0c.avfx" },
                [78u] = new HashSet<string> { "vfx/lockon/eff/m0462trg_a0c.avfx", "vfx/lockon/eff/m0462trg_b0c.avfx" }
            };
            if (Plugin.Config.DancingMadBlockShockwave)
                map[174u] = new HashSet<string> { "vfx/monster/m0462/eff/m0462hide_sp03c0c.avfx" };
            return map;
        }
    }
}
