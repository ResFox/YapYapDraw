using System.Collections.Generic;
using YapYapDraw.Engine.Interop;
using FFXIVClientStructs.FFXIV.Client.UI;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;
using YapYapDraw.Engine.Util;

namespace YapYapDraw.Modules.M2S;

public class BeeLovedVenomB : ISpecialAction
{
    public override string Name => "Bee-loved Venom β (buff)";

    public override uint Phase => 4u;

    public override HashSet<uint> ActionID => new HashSet<uint>();

    public override void OnAddStatus(ActorStatusChangeInfo info)
    {
        if (info.StatusID != 3933)
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
    }
}
