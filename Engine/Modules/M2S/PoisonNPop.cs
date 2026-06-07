using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using YapYapDraw.Engine.Interop;
using FFXIVClientStructs.FFXIV.Client.UI;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;
using YapYapDraw.Engine.Util;

namespace YapYapDraw.Modules.M2S;

public class PoisonNPop : ISpecialAction
{
    public override string Name => "Poison N Pop (buff)";

    public override uint Phase => 3u;

    public override HashSet<uint> ActionID => new HashSet<uint>();

    public override void OnAddStatus(ActorStatusChangeInfo info)
    {
        if (info.StatusID != 3934)
        {
            return;
        }
        if (info.Time == 26f)
        {
            if (info.TargetID == ((IGameObject)Svc.Objects.LocalPlayer).GameObjectId)
            {
                SimpleElement.ShowText("Short debuff", (TextGimmickHintStyle)0);
            }
        }
        else if (info.Time == 46f && info.TargetID == ((IGameObject)Svc.Objects.LocalPlayer).GameObjectId)
        {
            SimpleElement.ShowText("Long debuff", (TextGimmickHintStyle)0);
        }
        SimpleElement.Circle(info.TargetID.GameObject(), 14f, 4000f, (info.Time - 4f) * 1000f);
    }
}
