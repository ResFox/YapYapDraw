using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.ClientState.Objects.Types;
using YapYapDraw.Engine.Interop;
using YapYapDraw.Engine.Interop.Game;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;
using YapYapDraw.Engine.Util;

namespace YapYapDraw.Modules.M9S;

public class SonicScreech : ISpecialAction
{
    private readonly uint[] buffs = new uint[8] { 4731u, 4732u, 4733u, 4734u, 4735u, 4736u, 4737u, 4738u };

    public override string Name => "Sonic Screech / Congregate";

    public override HashSet<uint> ActionID => new HashSet<uint> { 45980u, 45981u };

    public override void OnActionCast(ActorCastInfo info)
    {
        if (info.ActionId == 45981)
        {
            IGameObject target = (IGameObject)Svc.Objects.LocalPlayer;
            if (((IGameObject?)Svc.Objects.LocalPlayer).HasStatus(buffs))
            {
                target = PlayerHelper.AllPlayers.FirstOrDefault((IGameObject x) => !x.HasStatus(buffs));
            }
            DrawManager.Draw(new DrawElement
            {
                drawAvfx = "gl_fan090_1bpxf",
                radiusX = 40f,
                radiusZ = 40f,
                drawOnObject = true,
                target = target,
                destroyTime = info.CastTime * 1000f,
                hitCounter = new HitCounter
                {
                    ActionID = new HashSet<uint> { 45983u }
                }
            }, info.SourceId.GameObject());
        }
        if (info.ActionId != 45980)
        {
            return;
        }
        IGameObject tankTarget = PlayerHelper.Tank.FirstOrDefault((IGameObject x) => !x.HasStatus(buffs));
        IGameObject healerTarget = PlayerHelper.Healer.FirstOrDefault((IGameObject x) => !x.HasStatus(buffs));
        IGameObject dpsTarget = PlayerHelper.DPS.FirstOrDefault((IGameObject x) => !x.HasStatus(buffs));
        switch (((ICharacter)Svc.Objects.LocalPlayer).GetRole())
        {
        case CombatRole.Tank:
        {
            IGameObject? source7 = info.SourceId.GameObject();
            IGameObject target4;
            if (!((IGameObject?)Svc.Objects.LocalPlayer).HasStatus(buffs))
            {
                IGameObject localPlayer = (IGameObject)Svc.Objects.LocalPlayer;
                target4 = localPlayer;
            }
            else
            {
                target4 = tankTarget;
            }
            HitCounter hitCounter = new HitCounter
            {
                ActionID = new HashSet<uint> { 45982u }
            };
            SimpleElement.FanToTarget(source7, target4, 40f, 100, Follow: true, default, 0f, 3000f, hitCounter);
            IGameObject? source8 = info.SourceId.GameObject();
            hitCounter = new HitCounter
            {
                ActionID = new HashSet<uint> { 45982u }
            };
            SimpleElement.FanToTarget(source8, healerTarget, 40f, 45, Follow: true, default, 0f, 3000f, hitCounter);
            IGameObject? source9 = info.SourceId.GameObject();
            hitCounter = new HitCounter
            {
                ActionID = new HashSet<uint> { 45982u }
            };
            SimpleElement.FanToTarget(source9, dpsTarget, 40f, 45, Follow: true, default, 0f, 3000f, hitCounter);
            break;
        }
        case CombatRole.Healer:
        {
            IGameObject? source4 = info.SourceId.GameObject();
            HitCounter hitCounter = new HitCounter
            {
                ActionID = new HashSet<uint> { 45982u }
            };
            SimpleElement.FanToTarget(source4, tankTarget, 40f, 100, Follow: true, default, 0f, 3000f, hitCounter);
            IGameObject? source5 = info.SourceId.GameObject();
            IGameObject target3;
            if (!((IGameObject?)Svc.Objects.LocalPlayer).HasStatus(buffs))
            {
                IGameObject localPlayer = (IGameObject)Svc.Objects.LocalPlayer;
                target3 = localPlayer;
            }
            else
            {
                target3 = healerTarget;
            }
            hitCounter = new HitCounter
            {
                ActionID = new HashSet<uint> { 45982u }
            };
            SimpleElement.FanToTarget(source5, target3, 40f, 45, Follow: true, default, 0f, 3000f, hitCounter);
            IGameObject? source6 = info.SourceId.GameObject();
            hitCounter = new HitCounter
            {
                ActionID = new HashSet<uint> { 45982u }
            };
            SimpleElement.FanToTarget(source6, dpsTarget, 40f, 45, Follow: true, default, 0f, 3000f, hitCounter);
            break;
        }
        case CombatRole.DPS:
        {
            IGameObject? source = info.SourceId.GameObject();
            HitCounter hitCounter = new HitCounter
            {
                ActionID = new HashSet<uint> { 45982u }
            };
            SimpleElement.FanToTarget(source, tankTarget, 40f, 100, Follow: true, default, 0f, 3000f, hitCounter);
            IGameObject? source2 = info.SourceId.GameObject();
            hitCounter = new HitCounter
            {
                ActionID = new HashSet<uint> { 45982u }
            };
            SimpleElement.FanToTarget(source2, healerTarget, 40f, 45, Follow: true, default, 0f, 3000f, hitCounter);
            IGameObject? source3 = info.SourceId.GameObject();
            IGameObject target2;
            if (!((IGameObject?)Svc.Objects.LocalPlayer).HasStatus(buffs))
            {
                IGameObject localPlayer = (IGameObject)Svc.Objects.LocalPlayer;
                target2 = localPlayer;
            }
            else
            {
                target2 = dpsTarget;
            }
            hitCounter = new HitCounter
            {
                ActionID = new HashSet<uint> { 45982u }
            };
            SimpleElement.FanToTarget(source3, target2, 40f, 45, Follow: true, default, 0f, 3000f, hitCounter);
            break;
        }
        }
    }
}
