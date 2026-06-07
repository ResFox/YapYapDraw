using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;
using YapYapDraw.Engine.Util;

namespace YapYapDraw.Modules.M8S;

public class WolvesReignConeCircle : ISpecialAction
{
    public override string Name => "Wolves' Reign(ConeCircle)";

    public override HashSet<uint> ActionID => new HashSet<uint> { 41880u, 42927u };

    public override void OnActionCast(ActorCastInfo info)
    {
        switch (info.ActionId)
        {
        case 41880:
            SimpleElement.Fan(info.Pos, 40f, 120, info.SourceId.GameObject().Rotation.Radians() + 180.Degrees(), 3000f, 0f, new HitCounter
            {
                ActionID = new HashSet<uint> { 42929u }
            });
            break;
        case 42927:
            SimpleElement.Circle(info.Pos, 14f, 3000f, 0f, new HitCounter
            {
                ActionID = new HashSet<uint> { 42930u }
            });
            break;
        }
    }

    public override void OnAbilityCast(ActorAbilityInfo info)
    {
        foreach (IGameObject tank in PlayerHelper.Tank)
        {
            DrawManager.Draw(new DrawElement
            {
                drawAvfx = "gl_fan060_1bf",
                radiusX = 40f,
                radiusZ = 40f,
                target = tank,
                delayDrawTime = 1000f,
                hitCounter = new HitCounter
                {
                    ActionID = new HashSet<uint> { 41883u }
                }
            }, info.Source);
        }
        foreach (IGameObject healer in PlayerHelper.Healer)
        {
            DrawManager.Draw(new DrawElement
            {
                drawAvfx = "gl_fan030_1bpf",
                radiusX = 40f,
                radiusZ = 40f,
                target = healer,
                delayDrawTime = 1000f,
                hitCounter = new HitCounter
                {
                    ActionID = new HashSet<uint> { 41884u }
                }
            }, info.Source);
        }
    }
}
