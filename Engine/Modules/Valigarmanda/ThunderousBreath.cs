using System.Collections.Generic;
using FFXIVClientStructs.FFXIV.Client.UI;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.Valigarmanda;

public class ThunderousBreath : ISpecialAction
{
    public override string Name => "Thunderous Breath";

    public override uint Phase => 2u;

    public override HashSet<uint> ActionID => new HashSet<uint> { 36835u };

    public override void OnActionCast(ActorCastInfo info)
    {
        SimpleElement.ShowText("Float up", (TextGimmickHintStyle)1, 8);
    }
}
