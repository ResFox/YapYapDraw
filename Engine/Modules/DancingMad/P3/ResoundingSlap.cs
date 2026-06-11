using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.ClientState.Objects.Types;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.Interop;
using YapYapDraw.Engine.Interop.Game;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;
using YapYapDraw.Engine.Util;

namespace YapYapDraw.Modules.DancingMad.P3;

public class ResoundingSlap : ISpecialAction
{
    public override string Name => "Resounding Slap";

    public override uint Phase => 3u;

    public override HashSet<uint> ActionID => new HashSet<uint> { 47846u, 47847u };

    public override void OnActionCast(ActorCastInfo info)
    {
        IGameObject? source = info.SourceId.GameObject();

        DrawElement element = new DrawElement
        {
            drawAvfx = "m0347_sircle_01m1",
            radiusX = 13f,
            radiusZ = 13f,
            refOffsetX = (info.ActionId == 47846) ? 10f : -10f,
            refOffsetZ = 15f,
            hitCounter = new HitCounter
            {
                ActionID = new HashSet<uint> { 47848u }
            }
        };
        DrawManager.Draw(element, source);
        element.refOffsetZ = 0f;
        element.hitCounter = new HitCounter
        {
            ActionID = new HashSet<uint> { 47848u },
            TargetHitCount = 2
        };
        DrawManager.Draw(element, source);
        element.refOffsetZ = -14f;
        element.hitCounter = new HitCounter
        {
            ActionID = new HashSet<uint> { 47848u },
            TargetHitCount = 3
        };
        DrawManager.Draw(element, source);

        if (info.ActionId == 47846)
        {
            DrawManager.Draw(new DrawElement
            {
                drawAvfx = "gl_fan060_1bpf",
                Position = info.Pos,
                drawOnObject = false,
                target = Svc.Objects.LocalPlayer,
                radiusX = 100f,
                radiusZ = 100f,
                hitCounter = new HitCounter
                {
                    ActionID = new HashSet<uint> { 47850u }
                }
            });
        }

        if (info.ActionId == 47847)
        {
            DrawElement fan = new DrawElement
            {
                drawAvfx = "gl_fan060_1bf",
                Position = info.Pos,
                drawOnObject = false,
                radiusX = 100f,
                radiusZ = 100f,
                hitCounter = new HitCounter
                {
                    ActionID = new HashSet<uint> { 47851u }
                }
            };

            CombatRole role = ((ICharacter)Svc.Objects.LocalPlayer).GetRole();
            if (role == CombatRole.Tank)
            {
                fan.target = PlayerHelper.Healer.FirstOrDefault();
                DrawManager.Draw(fan);
                fan.target = PlayerHelper.DPS.FirstOrDefault();
                DrawManager.Draw(fan);
            }
            else if (role == CombatRole.Healer)
            {
                fan.target = PlayerHelper.Tank.FirstOrDefault();
                DrawManager.Draw(fan);
                fan.target = PlayerHelper.DPS.FirstOrDefault();
                DrawManager.Draw(fan);
            }
            else
            {
                fan.target = PlayerHelper.Tank.FirstOrDefault();
                DrawManager.Draw(fan);
                fan.target = PlayerHelper.Healer.FirstOrDefault();
                DrawManager.Draw(fan);
            }

            fan.drawAvfx = "gl_fan060_1bpf";
            fan.target = Svc.Objects.LocalPlayer;
            DrawManager.Draw(fan);
        }
    }
}
