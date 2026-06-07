using System.Collections.Generic;
using System.Numerics;
using Dalamud.Game.ClientState.Objects.Types;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;
using YapYapDraw.Engine.Util;

namespace YapYapDraw.Modules.M1S;

public class LeapingQuadrupleCrossingSoul : ISpecialAction
{
    private Vector3 offset = new Vector3(-10f, 0f, 0f);

    public override string Name => "Leaping Quadruple Crossing (soul)";

    public override uint Phase => 1u;

    public override HashSet<uint> ActionID => new HashSet<uint> { 38995u, 38009u };

    public override void OnActionCast(ActorCastInfo info)
    {
        IGameObject source = info.SourceId.GameObject();
        Vector3 position = source.Position;
        float rotation = source.Rotation;
        if (rotation > 1f || rotation < -1f)
        {
            if (info.ActionId == 38995)
            {
                position -= offset;
            }
            else
            {
                position += offset;
            }
        }
        else if (info.ActionId != 38995)
        {
            position -= offset;
        }
        else
        {
            position += offset;
        }
        foreach (IGameObject allPlayer in PlayerHelper.AllPlayers)
        {
            DrawManager.Draw(new DrawElement
            {
                drawAvfx = "gl_fan045_1bf",
                Position = position,
                drawOnObject = false,
                target = allPlayer,
                radiusX = 100f,
                radiusZ = 100f,
                distanceCheck = new DistanceCheck
                {
                    Position = position,
                    CheckType = 4,
                    Count = 4
                },
                hitCounter = new HitCounter
                {
                    ActionID = new HashSet<uint> { 38013u },
                    TargetHitCount = 8
                }
            }, source);
        }
    }
}
