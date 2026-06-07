using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using YapYapDraw.Engine.Interop;
using FFXIVClientStructs.FFXIV.Client.UI;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.M4S;

public class LightningCageSpark : ISpecialAction
{
    public override string Name => "Ion Cluster (debuff)";

    public override uint Phase => 2u;

    public override HashSet<uint> ActionID => new HashSet<uint>();

    public override void OnAddStatus(ActorStatusChangeInfo info)
    {
        if (info.StatusID != 3999)
        {
            return;
        }
        ulong targetID = info.TargetID;
        IPlayerCharacter localPlayer = Svc.Objects.LocalPlayer;
        if (targetID != ((localPlayer != null) ? new ulong?(((IGameObject)localPlayer).GameObjectId) : ((ulong?)null)))
        {
            return;
        }
        float time = info.Time;
        if (time != 22f)
        {
            if (time == 42f)
            {
                SimpleElement.ShowText("Long debuff", (TextGimmickHintStyle)1);
            }
        }
        else
        {
            SimpleElement.ShowText("Short debuff", (TextGimmickHintStyle)1);
        }
        new TimeHelper((long)((info.Time - 7f) * 1000f), () =>
        {
            int playerIndex = WitchGleam.Players.FindIndex((IGameObject x) => x.GameObjectId == info.TargetID);
            int unused = 0;
            switch (WitchGleam.Stacks[playerIndex] switch
            {
                1 => 12, 
                2 => (info.Time > 30f) ? 20 : 12, 
                3 => 20, 
                _ => 0, 
            })
            {
            case 12:
                SimpleElement.ShowText("Small thunder — go inside", (TextGimmickHintStyle)1, 7);
                break;
            case 20:
                SimpleElement.ShowText("Big thunder — go corner", (TextGimmickHintStyle)0, 7);
                break;
            }
        });
    }
}
