using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using YapYapDraw.Engine.Interop;
using FFXIVClientStructs.FFXIV.Client.UI;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;
using YapYapDraw.Engine.Util;

namespace YapYapDraw.Modules.M3S;

public class Explosion : ISpecialAction
{
    public override string Name => "Fuses of Fury (buff)";

    public override uint Phase => 2u;

    public override HashSet<uint> ActionID => new HashSet<uint>();

    public override void OnAddStatus(ActorStatusChangeInfo info)
    {
        switch (info.StatusID)
        {
        case 4024u:
            if (info.TargetID == ((IGameObject)Svc.Objects.LocalPlayer).GameObjectId)
            {
                SimpleElement.ShowText("Short line", (TextGimmickHintStyle)0);
            }
            break;
        case 4025u:
            if (info.TargetID == ((IGameObject)Svc.Objects.LocalPlayer).GameObjectId)
            {
                SimpleElement.ShowText("Long line", (TextGimmickHintStyle)0);
            }
            break;
        case 4026u:
            SimpleLockon.TarLockOn6m5s(info.TargetID.GameObject());
            break;
        case 4027u:
            SimpleLockon.TarLockOn6m5s(info.TargetID.GameObject(), 5000f);
            break;
        }
    }
}
