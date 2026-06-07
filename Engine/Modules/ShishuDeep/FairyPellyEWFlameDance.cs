using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;
using YapYapDraw.Engine.Util;

namespace YapYapDraw.Modules.ShishuDeep;

public class FairyPellyEWFlameDance : ISpecialAction
{
    private bool? isLeft;

    public override string Name => "Fairy Pelly E/W Flame Dance";

    public override HashSet<uint> ActionID
    {
        get
        {
            HashSet<uint> hashSet = new HashSet<uint>();
            hashSet.Add(45426u);
            hashSet.Add(45427u);
            foreach (uint item in carpetRushIds)
            {
                hashSet.Add(item);
            }
            return hashSet;
        }
    }

    private static HashSet<uint> carpetRushIds => new HashSet<uint> { 45432u, 45433u, 45442u, 45443u, 46573u, 46574u, 46950u, 46951u, 47020u, 47021u };

    public override void OnActionCast(ActorCastInfo info)
    {
        switch (info.ActionId)
        {
        case 45426:
            isLeft = false;
            break;
        case 45427:
            isLeft = true;
            break;
        }
        base.NumCasts = 0;
    }

    public override void OnActorTetherEvent(uint actorId, uint Id, ulong targetId)
    {
        if (Id == 355 && isLeft.HasValue)
        {
            base.NumCasts++;
            DrawManager.Draw(new DrawElement
            {
                drawAvfx = "general02xf",
                Position = actorId.GameObject().Position,
                drawOnObject = false,
                targetPosition = targetId.GameObject().Position,
                radiusX = 40f,
                radiusZ = 40f,
                refOffsetZ = -40f,
                refOffsetRotation = (isLeft.Value ? 90.Degrees() : (-90.Degrees())),
                hitCounter = new HitCounter
                {
                    ActionID = carpetRushIds,
                    TargetHitCount = base.NumCasts
                }
            });
        }
    }

    public override void OnAbilityCast(ActorAbilityInfo info)
    {
        if (carpetRushIds.Contains(info.ActionId))
        {
            Reset();
        }
    }

    public override void Reset()
    {
        isLeft = null;
        base.Reset();
    }
}
