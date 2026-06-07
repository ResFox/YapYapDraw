using System.Collections.Generic;
using FFXIVClientStructs.FFXIV.Client.UI;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.Valigarmanda;

public class FreezingDust : ISpecialAction
{
    public override string Name => "Freezing Dust";

    public override uint Phase => 3u;

    public override HashSet<uint> ActionID => new HashSet<uint> { 36848u };

    public override void OnActionCast(ActorCastInfo info)
    {
        SimpleElement.ShowText("Move now!", (TextGimmickHintStyle)1, 7);
    }
}
