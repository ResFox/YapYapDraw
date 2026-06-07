using System.Collections.Generic;
using System.Linq;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;
using YapYapDraw.Engine.Util;
using YapYapDraw.Engine.Vfx;

namespace YapYapDraw.Modules.M4;

public class WickedCannon : ISpecialAction
{
    private static readonly HashSet<uint> CastEnd = new HashSet<uint> { 20032u, 37550u, 37551u, 39852u, 39870u };

    private static int HitCount = 0;

    public override string Name => "Wicked Cannon";

    public override HashSet<uint> ActionID
    {
        get
        {
            HashSet<uint> ids = new HashSet<uint>();
            ids.Add(37549u);
            ids.Add(37552u);
            ids.Add(39759u);
            ids.Add(39765u);
            ids.Add(39766u);
            ids.Add(39767u);
            foreach (uint id in CastEnd)
            {
                ids.Add(id);
            }
            return ids;
        }
    }

    public override IEnumerable<StaticVfx> ActiveAOEs => aoes.Take(1);

    public override void OnActionCast(ActorCastInfo info)
    {
        base.NumCasts = 0;
        int hitCount;
        switch (info.ActionId)
        {
        case 37549:
        case 37552:
            hitCount = 3;
            break;
        case 39759:
        case 39765:
            hitCount = 4;
            break;
        case 39766:
        case 39767:
            hitCount = 5;
            break;
        default:
            hitCount = 0;
            break;
        }
        HitCount = hitCount;
    }

    public override void OnAddStatus(ActorStatusChangeInfo info)
    {
        if (info.StatusID == 2970 && HitCount > base.NumCasts)
        {
            base.NumCasts++;
            Angle refRotation = info.Stack switch
            {
                723u => 180.Degrees(), 
                724u => 0.Degrees(), 
                _ => 0.Degrees(), 
            };
            DrawElement element = new DrawElement
            {
                drawAvfx = "general02xf",
                radiusX = 5f,
                radiusZ = 40f,
                drawOnObject = true,
                refRotation = refRotation,
                fixRotation = true,
                hitCounter = new HitCounter
                {
                    ActionID = CastEnd,
                    TargetHitCount = base.NumCasts
                }
            };
            aoes.Add(DrawManager.Draw(element, info.TargetID.GameObject()));
        }
    }

    public override void OnAbilityCast(ActorAbilityInfo info)
    {
        if (CastEnd.Contains(info.ActionId) && aoes.Count > 0)
        {
            aoes.RemoveAt(0);
        }
    }
}
