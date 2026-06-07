using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using YapYapDraw.Engine.Interop;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.M6S;

public class Wingmark : ISpecialAction
{
    public override string Name => "Wingmark";

    public override HashSet<uint> ActionID => new HashSet<uint>();

    public override void OnAddStatus(ActorStatusChangeInfo info)
    {
        if (info.StatusID == 4450)
        {
            ulong targetID = info.TargetID;
            IPlayerCharacter localPlayer = Svc.Objects.LocalPlayer;
            if (targetID == ((localPlayer != null) ? new ulong?(((IGameObject)localPlayer).GameObjectId) : ((ulong?)null)))
            {
                DrawManager.Draw(new DrawElement
                {
                    drawAvfx = "e5d1_b1_kblaser_t1",
                    radiusX = 1f,
                    radiusZ = 34f,
                    drawOnObject = true,
                    destroyTime = info.Time * 1000f,
                    StatusCheck = new StatusCheck
                    {
                        CheckObject = (IGameObject)Svc.Objects.LocalPlayer,
                        Status = 4450u
                    }
                }, (IGameObject?)Svc.Objects.LocalPlayer);
            }
        }
    }
}
