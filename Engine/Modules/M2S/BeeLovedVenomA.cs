using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.ClientState.Statuses;
using YapYapDraw.Engine.Interop;
using FFXIVClientStructs.FFXIV.Client.UI;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Enum;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;
using YapYapDraw.Engine.Util;

namespace YapYapDraw.Modules.M2S;

public class BeeLovedVenomA : ISpecialAction
{
    public override string Name => "Bee-loved Venom α (tether)";

    public override uint Phase => 4u;

    public override HashSet<uint> ActionID => new HashSet<uint>();

    public override void OnAddStatus(ActorStatusChangeInfo info)
    {
        if (info.StatusID != 3932)
        {
            return;
        }
        if (info.Time == 12f)
        {
            if (Svc.Objects.LocalPlayer == info.TargetID.GameObject())
            {
                SimpleElement.ShowText("Poison 1", (TextGimmickHintStyle)0);
            }
            SimpleLockon.Dice1_5s(info.TargetID.GameObject());
        }
        else if (info.Time == 28f)
        {
            if (Svc.Objects.LocalPlayer == info.TargetID.GameObject())
            {
                SimpleElement.ShowText("Poison 2", (TextGimmickHintStyle)0);
            }
            SimpleLockon.Dice2_5s(info.TargetID.GameObject());
        }
        else if (info.Time == 44f)
        {
            if (Svc.Objects.LocalPlayer == info.TargetID.GameObject())
            {
                SimpleElement.ShowText("Poison 3", (TextGimmickHintStyle)0);
            }
            SimpleLockon.Dice3_5s(info.TargetID.GameObject());
        }
        else if (info.Time == 62f)
        {
            if (Svc.Objects.LocalPlayer == info.TargetID.GameObject())
            {
                SimpleElement.ShowText("Poison 4", (TextGimmickHintStyle)0);
            }
            SimpleLockon.Dice4_5s(info.TargetID.GameObject());
        }
        new TimeHelper((long)(info.Time - 8f) * 1000, () =>
        {
            IPlayerCharacter recipient = (from player in PlayerHelper.AllPlayers.OfType<IPlayerCharacter>()
                where ((IBattleChara)player).StatusList.Any((IStatus status) => status.StatusId == 3933)
                orderby ((IBattleChara)player).StatusList.First((IStatus status) => status.StatusId == 3933).RemainingTime
                select player).FirstOrDefault();
            if (recipient != null)
            {
                DrawManager.Draw(new DrawElement
                {
                    drawType = ElementType.Channeling,
                    drawAvfx = "chan_dna_recombinant_ok0k1",
                    drawOnObject = true,
                    target = (IGameObject?)recipient,
                    delayDrawTime = (info.Time - 8f) * 1000f,
                    destroyTime = 8000f,
                    StatusCheck = new StatusCheck
                    {
                        CheckObject = info.TargetID.GameObject(),
                        Status = 3932u
                    }
                }, info.TargetID.GameObject(), (IGameObject?)recipient);
            }
        });
    }
}
