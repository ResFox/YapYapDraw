using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using YapYapDraw.Engine.Interop;
using FFXIVClientStructs.FFXIV.Client.UI;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.FRU;

public class DiscordantStrikeLightDarkLineStack : ISpecialAction
{
    public override string Name => "Discordant Strike (light/dark line stack)";

    public override uint Phase => 5u;

    public override HashSet<uint> ActionID => new HashSet<uint>();

    public override void OnAddStatus(ActorStatusChangeInfo info)
    {
        uint statusID = info.StatusID;
        bool isSwapStatus = statusID == 3323 || statusID == 4164;
        if (isSwapStatus && info.TargetID == ((IGameObject)Svc.Objects.LocalPlayer).GameObjectId)
        {
            SimpleElement.ShowText("Swap sides!", (TextGimmickHintStyle)0, 3);
        }
    }
}
