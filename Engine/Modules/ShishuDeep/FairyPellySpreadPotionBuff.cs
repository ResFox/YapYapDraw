using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.ShishuDeep;

public class FairyPellySpreadPotionBuff : ISpecialAction
{
    public override string Name => "Fairy Pelly Spread Potion (buff)";

    public override HashSet<uint> ActionID => new HashSet<uint>();

    public override void OnAddStatus(ActorStatusChangeInfo info)
    {
        if (info.StatusID == 4615)
        {
            SimpleElement.Circle(info.TargetID, 15f, 5000f, (info.Time - 5f) * 1000f);
        }
    }
}
