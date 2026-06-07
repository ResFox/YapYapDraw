using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using YapYapDraw.Engine.Interop;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.M5S;

public class BurnBabyBurn : ISpecialAction
{
    public override string Name => "BurnBabyBurn";

    public override uint Phase => 1u;

    public override HashSet<uint> ActionID => new HashSet<uint>();

    public override void OnAddStatus(ActorStatusChangeInfo info)
    {
        if (info.StatusID != 4461)
        {
            return;
        }
        ulong targetID = info.TargetID;
        IPlayerCharacter localPlayer = Svc.Objects.LocalPlayer;
        if (targetID == ((localPlayer != null) ? new ulong?(((IGameObject)localPlayer).GameObjectId) : ((ulong?)null)))
        {
            float time = info.Time;
            if (time == 9.5f || time == 23.5f)
            {
                SimpleLockon.Dice1_5s((IGameObject)Svc.Objects.LocalPlayer);
            }
            else
            {
                SimpleLockon.Dice2_5s((IGameObject)Svc.Objects.LocalPlayer);
            }
        }
    }
}
