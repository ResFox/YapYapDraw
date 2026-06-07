using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using YapYapDraw.Engine.Interop;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Enum;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.TsukuyomiUnreal;

public class Nightfall : ISpecialAction
{
    public override string Name => "Nightfall";

    public override HashSet<uint> ActionID => new HashSet<uint> { 45360u, 45361u };

    public override void OnAbilityCast(ActorAbilityInfo info)
    {
        if (info.ActionId == 45360)
        {
            DrawManager.Draw(new DrawElement
            {
                drawAvfx = "general02pxf",
                radiusX = 4f,
                radiusZ = 40f,
                drawOnObject = true,
                target = (IGameObject?)Svc.Objects.LocalPlayer,
                hitCounter = new HitCounter
                {
                    ActionID = new HashSet<uint> { 45362u }
                }
            }, info.Source);
            DrawManager.Draw(new DrawElement
            {
                drawType = ElementType.LockOn,
                drawAvfx = "share_laser_8sec_0t"
            }, (IGameObject?)Svc.Objects.LocalPlayer, info.Source);
        }
        if (info.ActionId == 45361)
        {
            DrawManager.Draw(new DrawElement
            {
                drawAvfx = "gl_fan090_1bpxf",
                radiusX = 40f,
                radiusZ = 40f,
                drawOnObject = true,
                target = (IGameObject?)Svc.Objects.LocalPlayer,
                hitCounter = new HitCounter
                {
                    ActionID = new HashSet<uint> { 45363u },
                    TargetHitCount = 3
                }
            }, info.Source);
        }
    }
}
