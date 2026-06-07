using System;
using System.Collections.Generic;
using System.Numerics;
using Dalamud.Game.ClientState.Objects.Types;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;
using YapYapDraw.Engine.Util;

namespace YapYapDraw.Modules.Fight_89740;

public class ChasmOfVollok : ISpecialAction
{
    private static readonly float PlatformOffset = 30f / MathF.Sqrt(2f);

    public override string Name => "Chasm of Vollok";

    public override HashSet<uint> ActionID => new HashSet<uint> { 37720u };

    public override uint Phase => 2u;

    public override void OnActionCast(ActorCastInfo info)
    {
        Vector3 center = new Vector3(100f, 0f, 100f);
        IGameObject source = info.SourceId.GameObject();
        if (!PositionHelper.IsPointInsideField(center, source.Position, 45f))
        {
            Vector3 offset = new Vector3((source.Position.X > center.X) ? (0f - PlatformOffset) : PlatformOffset, 0f, (source.Position.Z > center.Z) ? (0f - PlatformOffset) : PlatformOffset);
            DrawManager.Draw(new DrawElement
            {
                drawAvfx = "d1070_laser_01c2",
                radiusX = 2.5f,
                radiusZ = 5f,
                Position = source.Position + offset,
                refOffsetZ = 2.5f,
                drawOnObject = false,
                refRotation = info.Facing,
                fixRotation = true,
                hitCounter = new HitCounter
                {
                    ActionID = new HashSet<uint> { 37720u, 37722u }
                }
            }, source);
        }
    }
}
