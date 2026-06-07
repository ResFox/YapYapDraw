using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using YapYapDraw.Engine.Interop;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;
using YapYapDraw.Engine.Util;

namespace YapYapDraw.Modules.ShishuVc;

public class MermaidDariaCharmingCommand : ISpecialAction
{
    public override string Name => "Mermaid Daria Charming Command";

    public override HashSet<uint> ActionID => new HashSet<uint>();

    public override void OnAddStatus(ActorStatusChangeInfo info)
    {
        uint statusID = info.StatusID;
        if (statusID - 2161 <= 3)
        {
            ulong targetID = info.TargetID;
            IPlayerCharacter localPlayer = Svc.Objects.LocalPlayer;
            if (targetID == ((localPlayer != null) ? new ulong?(((IGameObject)localPlayer).GameObjectId) : ((ulong?)null)))
            {
                Angle refRotation = info.StatusID switch
                {
                    2161u => 0.Degrees(), 
                    2162u => 180.Degrees(), 
                    2163u => 90.Degrees(), 
                    2164u => -90.Degrees(), 
                    _ => 0.Degrees(), 
                };
                DrawManager.Draw(new DrawElement
                {
                    drawAvfx = "e5d1_b1_kblaser_t1",
                    radiusX = 1f,
                    radiusZ = 20f,
                    drawOnObject = true,
                    refRotation = refRotation,
                    destroyTime = info.Time * 1000f,
                    StatusCheck = new StatusCheck
                    {
                        CheckObject = (IGameObject)Svc.Objects.LocalPlayer,
                        Status = info.StatusID
                    }
                }, (IGameObject?)Svc.Objects.LocalPlayer);
            }
        }
    }
}
