using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using YapYapDraw.Engine.Interop;
using FFXIVClientStructs.FFXIV.Client.UI;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;
using YapYapDraw.Engine.Util;

namespace YapYapDraw.Modules.M3S;

public class Bombarium : ISpecialAction
{
    public override string Name => "Bombarium";

    public override uint Phase => 2u;

    public override HashSet<uint> ActionID => new HashSet<uint>();

    public override void OnAddStatus(ActorStatusChangeInfo info)
    {
        if (info.StatusID != 4020)
        {
            return;
        }
        float time = info.Time;
        if (time != 26f)
        {
            if (time == 44f)
            {
                if (info.TargetID == ((IGameObject)Svc.Objects.LocalPlayer).GameObjectId)
                {
                    SimpleElement.ShowText("Long debuff", (TextGimmickHintStyle)0);
                }
                SimpleLockon.Dice2_5s(info.TargetID.GameObject());
            }
        }
        else
        {
            if (info.TargetID == ((IGameObject)Svc.Objects.LocalPlayer).GameObjectId)
            {
                SimpleElement.ShowText("Short debuff", (TextGimmickHintStyle)0);
            }
            SimpleLockon.Dice1_5s(info.TargetID.GameObject());
        }
    }
}
