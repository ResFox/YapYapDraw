using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.ClientState.Statuses;
using YapYapDraw.Engine.Interop;
using YapYapDraw.Engine.Interop.Game;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.TEA;

public class TimeStopGuide : ISpecialAction
{
    public override string Name => "Time Stop Guide";

    public override uint Phase => 3u;

    public override HashSet<uint> ActionID => new HashSet<uint> { 18522u };

    public override void OnActionCast(ActorCastInfo info)
    {
        bool isRight = Svc.Objects.FirstOrDefault((IGameObject o) => o.BaseId == 11342).Position.X - 100f > 0f;
        if (((IBattleChara)Svc.Objects.LocalPlayer).StatusList.Any((IStatus status) => status.StatusId == 1121))
        {
            DrawElement element = new DrawElement
            {
                drawAvfx = "share_trap01k1",
                Position = new Vector3(isRight ? 81.5f : 118.5f, 0f, 100f),
                drawOnObject = false,
                radiusX = 2f,
                radiusY = 5f,
                radiusZ = 2f,
                hitCounter = new HitCounter
                {
                    ActionID = new HashSet<uint> { 18522u }
                }
            };
            DrawElement element2 = new DrawElement
            {
                drawAvfx = "customRect",
                Position = new Vector3(isRight ? 81.5f : 118.5f, 0f, 100f),
                drawOnObject = false,
                radiusX = 1f,
                target = (IGameObject?)Svc.Objects.LocalPlayer,
                endToTarget = true,
                refColor = new Vector4(1f, 1f, 0f, 1f),
                refTargetColor = new Vector4(1f, 1f, 0f, 1f),
                hitCounter = new HitCounter
                {
                    ActionID = new HashSet<uint> { 18522u }
                }
            };
            DrawManager.Draw(element, (IGameObject?)Svc.Objects.LocalPlayer);
            DrawManager.Draw(element2, (IGameObject?)Svc.Objects.LocalPlayer);
        }
        else if (((IBattleChara)Svc.Objects.LocalPlayer).StatusList.Any((IStatus status) => status.StatusId == 1123))
        {
            DrawElement element3 = new DrawElement
            {
                drawAvfx = "share_trap01k1",
                Position = new Vector3(106f, 0f, (((ICharacter)Svc.Objects.LocalPlayer).GetRole() == CombatRole.DPS) ? 101.5f : 98.5f),
                drawOnObject = false,
                radiusX = 1f,
                radiusY = 5f,
                radiusZ = 1f,
                hitCounter = new HitCounter
                {
                    ActionID = new HashSet<uint> { 18522u }
                }
            };
            DrawElement element4 = new DrawElement
            {
                drawAvfx = "customRect",
                Position = new Vector3(106f, 0f, (((ICharacter)Svc.Objects.LocalPlayer).GetRole() == CombatRole.DPS) ? 101.5f : 98.5f),
                drawOnObject = false,
                radiusX = 1f,
                target = (IGameObject?)Svc.Objects.LocalPlayer,
                endToTarget = true,
                refColor = new Vector4(1f, 1f, 0f, 1f),
                refTargetColor = new Vector4(1f, 1f, 0f, 1f),
                hitCounter = new HitCounter
                {
                    ActionID = new HashSet<uint> { 18522u }
                }
            };
            DrawManager.Draw(element3, (IGameObject?)Svc.Objects.LocalPlayer);
            DrawManager.Draw(element4, (IGameObject?)Svc.Objects.LocalPlayer);
        }
        else if (((IBattleChara)Svc.Objects.LocalPlayer).StatusList.Any((IStatus status) => status.StatusId == 1124))
        {
            if (isRight)
            {
                DrawElement element5 = new DrawElement
                {
                    drawAvfx = "share_trap01k1",
                    Position = new Vector3((((ICharacter)Svc.Objects.LocalPlayer).GetRole() == CombatRole.DPS) ? 115.6f : 81.5f, 0f, 100f),
                    drawOnObject = false,
                    radiusX = 1f,
                    radiusY = 5f,
                    radiusZ = 1f,
                    hitCounter = new HitCounter
                    {
                        ActionID = new HashSet<uint> { 18522u }
                    }
                };
                DrawElement element6 = new DrawElement
                {
                    drawAvfx = "customRect",
                    Position = new Vector3((((ICharacter)Svc.Objects.LocalPlayer).GetRole() == CombatRole.DPS) ? 115.6f : 81.5f, 0f, 100f),
                    drawOnObject = false,
                    radiusX = 1f,
                    target = (IGameObject?)Svc.Objects.LocalPlayer,
                    endToTarget = true,
                    refColor = new Vector4(1f, 1f, 0f, 1f),
                    refTargetColor = new Vector4(1f, 1f, 0f, 1f),
                    hitCounter = new HitCounter
                    {
                        ActionID = new HashSet<uint> { 18522u }
                    }
                };
                DrawManager.Draw(element5, (IGameObject?)Svc.Objects.LocalPlayer);
                DrawManager.Draw(element6, (IGameObject?)Svc.Objects.LocalPlayer);
            }
            else
            {
                DrawElement element7 = new DrawElement
                {
                    drawAvfx = "share_trap01k1",
                    Position = new Vector3((((ICharacter)Svc.Objects.LocalPlayer).GetRole() == CombatRole.DPS) ? 118.5f : 84.5f, 0f, 100f),
                    drawOnObject = false,
                    radiusX = 1f,
                    radiusY = 5f,
                    radiusZ = 1f,
                    hitCounter = new HitCounter
                    {
                        ActionID = new HashSet<uint> { 18522u }
                    }
                };
                DrawElement element8 = new DrawElement
                {
                    drawAvfx = "customRect",
                    Position = new Vector3((((ICharacter)Svc.Objects.LocalPlayer).GetRole() == CombatRole.DPS) ? 118.5f : 84.5f, 0f, 100f),
                    drawOnObject = false,
                    radiusX = 1f,
                    target = (IGameObject?)Svc.Objects.LocalPlayer,
                    endToTarget = true,
                    refColor = new Vector4(1f, 1f, 0f, 1f),
                    refTargetColor = new Vector4(1f, 1f, 0f, 1f),
                    hitCounter = new HitCounter
                    {
                        ActionID = new HashSet<uint> { 18522u }
                    }
                };
                DrawManager.Draw(element7, (IGameObject?)Svc.Objects.LocalPlayer);
                DrawManager.Draw(element8, (IGameObject?)Svc.Objects.LocalPlayer);
            }
        }
        else
        {
            DrawElement element9 = new DrawElement
            {
                drawAvfx = "share_trap01k1",
                Position = new Vector3(94f, 0f, (((ICharacter)Svc.Objects.LocalPlayer).GetRole() == CombatRole.DPS) ? 101.5f : 98.5f),
                drawOnObject = false,
                radiusX = 1f,
                radiusY = 5f,
                radiusZ = 1f,
                hitCounter = new HitCounter
                {
                    ActionID = new HashSet<uint> { 18522u }
                }
            };
            DrawElement element10 = new DrawElement
            {
                drawAvfx = "customRect",
                Position = new Vector3(94f, 0f, (((ICharacter)Svc.Objects.LocalPlayer).GetRole() == CombatRole.DPS) ? 101.5f : 98.5f),
                drawOnObject = false,
                radiusX = 1f,
                target = (IGameObject?)Svc.Objects.LocalPlayer,
                endToTarget = true,
                refColor = new Vector4(1f, 1f, 0f, 1f),
                refTargetColor = new Vector4(1f, 1f, 0f, 1f),
                hitCounter = new HitCounter
                {
                    ActionID = new HashSet<uint> { 18522u }
                }
            };
            DrawManager.Draw(element9, (IGameObject?)Svc.Objects.LocalPlayer);
            DrawManager.Draw(element10, (IGameObject?)Svc.Objects.LocalPlayer);
        }
    }
}
