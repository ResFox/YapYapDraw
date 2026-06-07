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

namespace YapYapDraw.Modules.Fight_33798;

public class Charge : ISpecialAction
{
    public override string Name => "Charge";

    public override HashSet<uint> ActionID => new HashSet<uint> { 43841u, 43842u, 43843u };

    public override void OnActionCast(ActorCastInfo info)
    {
        if (info.ActionId == 43841)
        {
            IGameObject source = info.SourceId.GameObject();
            Vector3 offset = info.TargetPos - source.Position;
            Angle refRotation = MathF.Atan2(offset.X, offset.Z).Radians();
            DrawElement element = new DrawElement
            {
                drawAvfx = "general02xf",
                Position = source.Position,
                drawOnObject = false,
                radiusX = 6f,
                refRotation = refRotation,
                fixRotation = true,
                targetPosition = info.TargetPos,
                endToTarget = true,
                hitCounter = new HitCounter
                {
                    ActionID = new HashSet<uint> { 43842u, 43843u },
                    TargetHitCount = 3
                }
            };
            aoes.Add(DrawManager.Draw(element));
        }
    }

    public override void OnAbilityCast(ActorAbilityInfo info)
    {
        uint actionId = info.ActionId;
        bool isFollowUp = actionId - 43842 <= 1;
        if (isFollowUp && aoes.Count > 0)
        {
            aoes[0].Remove();
            aoes.RemoveAt(0);
        }
    }
}
