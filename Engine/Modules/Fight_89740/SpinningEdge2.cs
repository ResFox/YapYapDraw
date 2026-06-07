using System;
using System.Collections.Generic;
using System.Numerics;
using Dalamud.Game.ClientState.Objects.Types;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Util;

namespace YapYapDraw.Modules.Fight_89740;

public class SpinningEdge2 : ISpecialAction
{
    public override string Name => "Spinning Edge";

    public override HashSet<uint> ActionID => new HashSet<uint>();

    public override uint Phase => 2u;

    public override void OnActorTetherEvent(uint actorId, uint Id, ulong targetId)
    {
        if (Id == 86)
        {
            Vector3 center = new Vector3(100f, 0f, 100f);
            IGameObject actor = actorId.GameObject();
            Vector3 offset = actor.Position - center;
            bool isDiagonal = actor.Rotation.Radians().AlmostEqual(-45f.Degrees(), (float)Math.PI / 180f) || actor.Rotation.Radians().AlmostEqual(135f.Degrees(), (float)Math.PI / 180f);
            Vector2 dir = new Vector2(0f - actor.Rotation.Radians().ToDirection().Z, actor.Rotation.Radians().ToDirection().X);
            float dist = offset.X * dir.X + offset.Z * dir.Y;
            bool isLowerBand = dist < -7f && dist > -8f;
            bool isUpperBand = dist > 2f && dist < 3f;
            float adjustedDist = dist + (float)(isDiagonal ? (5 * ((isLowerBand || isUpperBand) ? 1 : (-1))) : 0);
            DrawManager.Draw(new DrawElement
            {
                drawAvfx = "general02xf",
                radiusX = 2.5f,
                radiusZ = 20f,
                Position = new Vector3(100f + dir.X * adjustedDist, 0f, 100f + dir.Y * adjustedDist),
                refOffsetZ = 10f,
                drawOnObject = false,
                refRotation = new Angle(actor.Rotation),
                fixRotation = true,
                destroyTime = 13300f
            }, actor);
        }
    }
}
