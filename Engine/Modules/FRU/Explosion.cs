using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.ClientState.Objects.Types;
using YapYapDraw.Engine.Interop;
using FFXIVClientStructs.FFXIV.Client.UI;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;
using YapYapDraw.Engine.Util;

namespace YapYapDraw.Modules.FRU;

public class Explosion : ISpecialAction
{
    public override string Name => "Wings of Light/Dark (tether)";

    public override uint Phase => 5u;

    public override HashSet<uint> ActionID => new HashSet<uint> { 40313u, 40233u };

    public override void OnActorTetherEvent(uint actorId, uint Id, ulong targetId)
    {
        if (Id == 1 || Id == 2)
        {
            DrawManager.Draw(new DrawElement
            {
                drawAvfx = "general_1bxf",
                radiusX = 3f,
                radiusZ = 3f,
                drawOnObject = true,
                distanceCheck = new DistanceCheck
                {
                    CheckObject = actorId.GameObject(),
                    CheckType = ((Id == 1) ? 2 : 3)
                },
                hitCounter = new HitCounter
                {
                    ActionID = new HashSet<uint> { 40320u }
                }
            }, targetId.GameObject());
        }
    }

    public override void OnActionCast(ActorCastInfo info)
    {
        if (PlayerHelper.Tank.FirstOrDefault((IGameObject o) => o != WingsOfLightDarkTankCleave.MT) == Svc.Objects.LocalPlayer)
        {
            switch (info.ActionId)
            {
            case 40313:
                SimpleElement.ShowText("Light — bait far", (TextGimmickHintStyle)0);
                break;
            case 40233:
                SimpleElement.ShowText("Dark — bait close", (TextGimmickHintStyle)0);
                break;
            }
        }
    }

    public override void OnAbilityCast(ActorAbilityInfo info)
    {
        if (WingsOfLightDarkTankCleave.MT == Svc.Objects.LocalPlayer)
        {
            switch (info.ActionId)
            {
            case 40313u:
                SimpleElement.ShowText("Dark — bait close", (TextGimmickHintStyle)0);
                break;
            case 40233u:
                SimpleElement.ShowText("Light — bait far", (TextGimmickHintStyle)0);
                break;
            }
        }
    }
}
