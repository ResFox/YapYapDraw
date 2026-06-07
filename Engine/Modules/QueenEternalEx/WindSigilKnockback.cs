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

namespace YapYapDraw.Modules.QueenEternalEx;
public class WindSigilKnockback : ISpecialAction
{
    public override string Name => "Wind Sigil (knockback)";

    public override HashSet<uint> ActionID => new HashSet<uint>();

    public override void OnAddStatus(ActorStatusChangeInfo info)
    {
        uint statusID = info.StatusID;
        if (statusID - 4189 <= 1)
        {
            ulong targetID = info.TargetID;
            IPlayerCharacter localPlayer = Svc.Objects.LocalPlayer;
            if (targetID == ((localPlayer != null) ? new ulong?(((IGameObject)localPlayer).GameObjectId) : ((ulong?)null)))
            {
                DrawManager.Draw(new DrawElement
                {
                    drawAvfx = "e5d1_b1_kblaser_t1",
                    radiusX = 1f,
                    radiusZ = 20f,
                    refRotation = ((info.StatusID == 4189) ? (-90.Degrees()) : 90.Degrees()),
                    fixRotation = true,
                    delayDrawTime = (info.Time - 5f) * 1000f,
                    hitCounter = new HitCounter
                    {
                        ActionID = new HashSet<uint> { 40996u }
                    }
                }, (IGameObject?)Svc.Objects.LocalPlayer);
            }
        }
    }
}
