using System.Collections.Generic;
using FFXIVClientStructs.FFXIV.Client.UI;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;
using YapYapDraw.Engine.Util;

namespace YapYapDraw.Modules.FRU;

public class DarkWaterIII : ISpecialAction
{
    public override string Name => "Dark Water III (stack)";

    public override uint Phase => 3u;

    public override HashSet<uint> ActionID => new HashSet<uint>();

    public override void OnAddStatus(ActorStatusChangeInfo info)
    {
        if (info.StatusID == 2461)
        {
            SimpleLockon.ShareLockon(info.TargetID.GameObject(), (info.Time - 5f) * 1000f);
            new TimeHelper((long)((info.Time - 5f) * 1000f), () =>
            {
                SimpleElement.ShowText("Real stack", (TextGimmickHintStyle)0);
            });
        }
    }
}
