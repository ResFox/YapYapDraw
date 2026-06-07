using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using YapYapDraw.Engine.Interop;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;
using YapYapDraw.Engine.Util;

namespace YapYapDraw.Modules.DancingMad.P1;

public class IdolTethers : ISpecialAction
{
    public override string Name => "Idol Tethers";

    public override HashSet<uint> ActionID => new HashSet<uint> { 47788u };

    public override void OnAbilityCast(ActorAbilityInfo info)
    {
        base.NumCasts++;
    }

    public override void OnActorTetherEvent(uint actorId, uint Id, ulong targetId)
    {
        if (Id == 45)
        {
            IGameObject? obj = actorId.GameObject();
            if (obj.Position.Y == 18.5f && targetId == ((IGameObject)Svc.Objects.LocalPlayer).GameObjectId)
            {
                DrawManager.Draw(new DrawElement
                {
                    drawAvfx = "e5d1_b1_kblaser_t1",
                    radiusX = 1f,
                    radiusZ = 10f,
                    fixRotation = true,
                    TetherCheck = new TetherCheck
                    {
                        CheckType = 1,
                        TetherID = new HashSet<int> { 45 }
                    },
                    hitCounter = new HitCounter
                    {
                        ActionID = new HashSet<uint> { 47785u }
                    }
                }, (IGameObject?)Svc.Objects.LocalPlayer);
            }
            if (obj.Position.Y == 7f)
            {
                SimpleLockon.TarLockOn5m5s(targetId.GameObject(), (base.NumCasts == 0) ? 5500 : 7500);
            }
            if (obj.Position.Y == 22.5f)
            {
                SimpleLockon.ShareLockon2(targetId.GameObject(), (base.NumCasts == 0) ? 1500 : 3500);
            }
        }
    }
}
