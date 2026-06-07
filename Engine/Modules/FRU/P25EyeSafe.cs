using System.Collections.Generic;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Util;

namespace YapYapDraw.Modules.FRU;

public class P25EyeSafe : ISpecialAction
{
    public override string Name => "P2.5 (eye-safe)";

    public override uint Phase => 2u;

    public override HashSet<uint> ActionID => new HashSet<uint>();

    public override void OnMapEffect(uint a2, ushort a3, ushort a4)
    {
        if (a2 == 24 && a3 == 1 && a4 == 2)
        {
            new TimeHelper(5000L, () =>
            {
                HookManager.ProcessMapEffectFunc(MapUtil.GetMapEffectModule(), 24u, 4, 8);
            });
        }
    }
}
