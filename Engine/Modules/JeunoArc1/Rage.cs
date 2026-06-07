using System;
using System.Collections.Generic;
using System.Numerics;
using Dalamud.Game.ClientState.Objects.Types;
using YapYapDraw.Engine.Interop;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;
using YapYapDraw.Engine.Util;

namespace YapYapDraw.Modules.JeunoArc1;

public class Rage : ISpecialAction
{
    public override string Name => "Rage";

    public override HashSet<uint> ActionID => new HashSet<uint> { 41073u, 41074u };

    public override uint Phase => 3u;

    public override void OnActionCast(ActorCastInfo info)
    {
        if (info.ActionId == 41073)
        {
            base.NumCasts++;
            Vector3 position = info.SourceId.GameObject().Position;
            Vector3 dir = info.Pos - position;
            DrawManager.Draw(new DrawElement
            {
                drawAvfx = "general02xf",
                Position = position,
                drawOnObject = false,
                radiusX = 5f,
                targetPosition = info.Pos,
                endToTarget = true,
                refRotation = new Angle(MathF.Atan2(dir.X, dir.Z)),
                fixRotation = true,
                hitCounter = new HitCounter
                {
                    ActionID = new HashSet<uint> { 41075u },
                    TargetHitCount = base.NumCasts
                }
            }, (IGameObject?)Svc.Objects.LocalPlayer);
        }
        else
        {
            SimpleElement.Circle(info.Pos, 20f, 3000f, 0f, new HitCounter
            {
                ActionID = new HashSet<uint> { 41077u }
            });
            base.NumCasts = 0;
        }
    }
}
