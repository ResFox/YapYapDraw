using System.Collections.Generic;
using FFXIVClientStructs.FFXIV.Client.UI;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.FRU;

public class ReturnSavedPosition : ISpecialAction
{
    public override string Name => "Return (saved position)";

    public override uint Phase => 3u;

    public override HashSet<uint> ActionID => new HashSet<uint>();

    public override void OnAddStatus(ActorStatusChangeInfo info)
    {
        if (info.StatusID == 4208)
        {
            new TimeHelper((long)((info.Time - 5f) * 1000f), () =>
            {
                SimpleElement.ShowText("Note your position", (TextGimmickHintStyle)0);
            });
        }
    }
}
