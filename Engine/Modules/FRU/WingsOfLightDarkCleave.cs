using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Game.ClientState.Objects.Types;
using YapYapDraw.Engine.Interop;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Enum;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;
using YapYapDraw.Engine.Util;

namespace YapYapDraw.Modules.FRU;

public class WingsOfLightDarkCleave : ISpecialAction
{
    public static bool LightFirst;

    private IGameObject? firstTarget;

    public override string Name => "Wings of Light/Dark (cleave)";

    public override uint Phase => 5u;

    public override HashSet<uint> ActionID => new HashSet<uint> { 40313u, 40233u };

    public override void OnActionCast(ActorCastInfo info)
    {
        LightFirst = info.ActionId == 40313;
        IGameObject boss = Svc.Objects.FirstOrDefault((IGameObject o) => o.BaseId == 17839);
        firstTarget = boss.TargetObject;
        DrawManager.Draw(new DrawElement
        {
            drawAvfx = "customFan",
            refRadian = 225f.Degrees().Rad,
            radiusX = 100f,
            radiusZ = 100f,
            refOffsetRotation = ((info.ActionId == 40313) ? 67.5f.Degrees() : (-67.5f.Degrees())),
            target = firstTarget,
            hitCounter = new HitCounter
            {
                ActionID = new HashSet<uint> { 40314u, 40315u }
            },
            refColor = ((info.ActionId == 40313) ? GroundOmen.enemyColor : new Vector4(0.94f, 0f, 1f, YapYapDraw.Plugin.Config.CustomAlpha)),
            refTargetColor = ((info.ActionId == 40313) ? GroundOmen.enemyColor : new Vector4(0.94f, 0f, 1f, YapYapDraw.Plugin.Config.CustomAlpha))
        }, boss);
    }

    public override void OnAbilityCast(ActorAbilityInfo info)
    {
        IGameObject target = Svc.Objects.FirstOrDefault((IGameObject o) => o.BaseId == 17839);
        IGameObject otherTank = PlayerHelper.Tank.FirstOrDefault((IGameObject o) => o != firstTarget);
        DrawManager.Draw(new DrawElement
        {
            drawAvfx = "customFan",
            refRadian = 225f.Degrees().Rad,
            radiusX = 100f,
            radiusZ = 100f,
            delayDrawTime = 1000f,
            refOffsetRotation = ((info.ActionId == 40313) ? (-67.5f.Degrees()) : 67.5f.Degrees()),
            target = otherTank,
            hitCounter = new HitCounter
            {
                ActionID = new HashSet<uint> { 40314u, 40315u },
                TargetHitCount = 2
            },
            refColor = ((info.ActionId == 40313) ? new Vector4(0.94f, 0f, 1f, YapYapDraw.Plugin.Config.CustomAlpha) : GroundOmen.enemyColor),
            refTargetColor = ((info.ActionId == 40313) ? new Vector4(0.94f, 0f, 1f, YapYapDraw.Plugin.Config.CustomAlpha) : GroundOmen.enemyColor)
        }, target);
    }
}
