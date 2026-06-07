using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Game.ClientState.Objects.Types;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;
using YapYapDraw.Engine.Util;
using YapYapDraw.Engine.Vfx;

namespace YapYapDraw.Modules.M1S;

public class LeapingOne_twoPaw : ISpecialAction
{
    private readonly HashSet<uint> castEnd = new HashSet<uint> { 37970u, 37971u, 37974u, 37973u, 38004u, 38005u, 38008u, 38007u };

    private Angle leapDirection;

    private Angle firstDirection;

    private IGameObject? clone;

    public override string Name => "Leaping One-two Paw";

    public override uint Phase => 3u;

    public override HashSet<uint> ActionID
    {
        get
        {
            HashSet<uint> ids = new HashSet<uint>();
            ids.Add(37965u);
            ids.Add(37966u);
            ids.Add(37967u);
            ids.Add(37968u);
            foreach (uint id in castEnd)
            {
                ids.Add(id);
            }
            return ids;
        }
    }

    public override IEnumerable<StaticVfx> ActiveAOEs => aoes.Take(1);

    public override void OnActionCast(ActorCastInfo info)
    {
        if (!castEnd.Contains(info.ActionId))
        {
            var (angle, angle2) = info.ActionId switch
            {
                37965 => (90.Degrees(), -90.Degrees()), 
                37966 => (90.Degrees(), 90.Degrees()), 
                37967 => (-90.Degrees(), -90.Degrees()), 
                37968 => (-90.Degrees(), 90.Degrees()), 
                _ => default((Angle, Angle)), 
            };
            if (!(angle == default))
            {
                leapDirection = angle;
                firstDirection = angle2;
                StartMechanic(info.SourceId.GameObject().Position, info.Facing);
            }
        }
    }

    public override void OnActorTetherEvent(uint actorId, uint Id, ulong targetId)
    {
        if (Id == 102 && leapDirection != default)
        {
            if (clone == null)
            {
                clone = actorId.GameObject();
            }
            else if (clone == actorId.GameObject())
            {
                StartMechanic(actorId.GameObject().Position, actorId.GameObject().Rotation.Radians());
            }
        }
    }

    private void StartMechanic(Vector3 position, Angle rotation)
    {
        WPos wPos = new WPos(position) + 10f * (rotation + leapDirection).ToDirection();
        DrawElement drawElement = new DrawElement
        {
            Enable = false,
            drawAvfx = "gl_fan180_1bf",
            Position = wPos.ToVec3(),
            drawOnObject = false,
            radiusX = 100f,
            radiusZ = 100f,
            refRotation = rotation + firstDirection,
            fixRotation = true,
            hitCounter = new HitCounter
            {
                ActionID = castEnd
            }
        };
        aoes.Add(DrawManager.Draw(drawElement));
        drawElement.refRotation = rotation - firstDirection;
        drawElement.hitCounter = new HitCounter
        {
            ActionID = castEnd,
            TargetHitCount = 2
        };
        aoes.Add(DrawManager.Draw(drawElement));
    }

    public override void OnAbilityCast(ActorAbilityInfo info)
    {
        if (castEnd.Contains(info.ActionId) && aoes.Count > 0)
        {
            aoes.RemoveAt(0);
        }
    }

    public override void Reset()
    {
        leapDirection = default;
        firstDirection = default;
        clone = null;
    }
}
