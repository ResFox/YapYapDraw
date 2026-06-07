using System;
using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.ClientState.Objects.Types;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;
using YapYapDraw.Engine.Util;
using YapYapDraw.Engine.Vfx;

namespace YapYapDraw.Modules.M9S;

public class BatStomp : ISpecialAction
{
    public override string Name => "Bat Stomp";

    public override HashSet<uint> ActionID => new HashSet<uint> { 45940u, 45941u };

    public override IEnumerable<StaticVfx> ActiveAOEs
    {
        get
        {
            if (aoes.Count == 0)
            {
                return Array.Empty<StaticVfx>();
            }
            int numCasts = base.NumCasts;
            return aoes.Take(numCasts switch
            {
                2 => 3, 
                5 => 5, 
                _ => 2, 
            });
        }
    }

    public override void OnAddStatus(ActorStatusChangeInfo info)
    {
        if (info.StatusID == 1957)
        {
            int targetHitCount = info.Stack switch
            {
                37u => 2, 
                51u => 5, 
                55u => 10, 
                _ => 0, 
            };
            IGameObject target = info.TargetID.GameObject();
            Angle dir = ((target.Rotation.Radians().ToDirection().OrthoL()
                .Dot(target.DirectionTo(new WPos(100f, 100f))) > 0f) ? 1 : (-1)) * 90.Degrees();
            WDir wDir = new WPos(target.Position) - new WPos(100f, 100f);
            WPos wPos = new WPos(100f, 100f) + wDir.Rotate(dir);
            DrawElement element = new DrawElement
            {
                drawAvfx = "general_1bxf",
                Position = wPos.ToVec3(),
                drawOnObject = false,
                radiusX = 8f,
                radiusZ = 8f,
                hitCounter = new HitCounter
                {
                    ActionID = new HashSet<uint> { 45941u },
                    TargetHitCount = targetHitCount
                }
            };
            aoes.Add(DrawManager.Draw(element));
            aoes.SortBy((StaticVfx v) => v.HitCounter.TargetHitCount);
        }
    }

    public override void OnActionCast(ActorCastInfo info)
    {
        if (info.ActionId == 45940)
        {
            Reset();
        }
    }

    public override void OnAbilityCast(ActorAbilityInfo info)
    {
        if (info.ActionId == 45941)
        {
            if (aoes.Count > 0)
            {
                aoes[0].Remove();
                aoes.RemoveAt(0);
            }
            base.NumCasts++;
        }
    }
}
