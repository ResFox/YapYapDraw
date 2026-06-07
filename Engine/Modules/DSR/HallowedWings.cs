using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.ClientState.Objects.Types;
using YapYapDraw.Engine.Interop;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;
using YapYapDraw.Engine.Util;

namespace YapYapDraw.Modules.DSR;

public class HallowedWings : ISpecialAction
{
    public override string Name => "Hallowed Wings";

    public override uint Phase => 6u;

    public override HashSet<uint> ActionID => new HashSet<uint> { 27939u, 27940u, 27942u, 27943u };

    public override void OnActionCast(ActorCastInfo info)
    {
        IGameObject source = Svc.Objects.SearchById((ulong)info.SourceId);
        Svc.Objects.Where((IGameObject obj) => (int)obj.ObjectKind == 1).ToList();
        if (source != null)
        {
            switch (info.ActionId)
            {
            case 27939:
                LeftHalfCleave(source);
                NearestTankbuster(source);
                break;
            case 27940:
                LeftHalfCleave(source);
                FarthestTankbuster(source);
                break;
            case 27942:
                RightHalfCleave(source);
                NearestTankbuster(source);
                break;
            case 27943:
                RightHalfCleave(source);
                FarthestTankbuster(source);
                break;
            case 27941:
                break;
            }
        }
    }

    private static void LeftHalfCleave(IGameObject sourceObject)
    {
        DrawManager.Draw(new DrawElement
        {
            drawAvfx = "general02xf",
            radiusX = 60f,
            radiusZ = 40f,
            refRotation = 90.Degrees(),
            drawOnObject = true,
            hitCounter = new HitCounter
            {
                ActionID = new HashSet<uint> { 27941u }
            }
        }, sourceObject);
    }

    private static void RightHalfCleave(IGameObject sourceObject)
    {
        DrawManager.Draw(new DrawElement
        {
            drawAvfx = "general02xf",
            radiusX = 60f,
            radiusZ = 40f,
            refRotation = -90.Degrees(),
            drawOnObject = true,
            hitCounter = new HitCounter
            {
                ActionID = new HashSet<uint> { 27944u }
            }
        }, sourceObject);
    }

    private static void NearestTankbuster(IGameObject sourceObject)
    {
        foreach (IGameObject item in Svc.Objects.Where((IGameObject obj) => (int)obj.ObjectKind == 1).ToList())
        {
            DrawManager.Draw(new DrawElement
            {
                drawAvfx = "general_1bxf",
                radiusX = 10f,
                radiusZ = 10f,
                drawOnObject = true,
                distanceCheck = new DistanceCheck
                {
                    CheckObject = sourceObject,
                    CheckType = 2,
                    Count = 2
                },
                hitCounter = new HitCounter
                {
                    ActionID = new HashSet<uint> { 27945u },
                    TargetHitCount = 2
                }
            }, item);
        }
    }

    private static void FarthestTankbuster(IGameObject sourceObject)
    {
        foreach (IGameObject item in Svc.Objects.Where((IGameObject obj) => (int)obj.ObjectKind == 1).ToList())
        {
            DrawManager.Draw(new DrawElement
            {
                drawAvfx = "general_1bxf",
                radiusX = 10f,
                radiusZ = 10f,
                drawOnObject = true,
                distanceCheck = new DistanceCheck
                {
                    CheckObject = sourceObject,
                    CheckType = 3,
                    Count = 2
                },
                hitCounter = new HitCounter
                {
                    ActionID = new HashSet<uint> { 27945u },
                    TargetHitCount = 2
                }
            }, item);
        }
    }
}
