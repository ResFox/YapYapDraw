using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using YapYapDraw.Engine.Interop;
using YapYapDraw.Engine.Interop.Game;
using FFXIVClientStructs.FFXIV.Client.UI;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;
using YapYapDraw.Engine.Util;

namespace YapYapDraw.Modules.FRU;

public class Quietus : ISpecialAction
{
    public override string Name => "Quietus";

    public override uint Phase => 4u;

    public override HashSet<uint> ActionID => new HashSet<uint> { 40283u, 40284u };

    public override void OnActionCast(ActorCastInfo info)
    {
        if (((ICharacter)Svc.Objects.LocalPlayer).GetRole() == CombatRole.Tank)
        {
            SimpleElement.ShowText("Bait execution", (TextGimmickHintStyle)0);
        }
        foreach (IGameObject allPlayer in PlayerHelper.AllPlayers)
        {
            DrawManager.Draw(new DrawElement
            {
                drawAvfx = "general_1bzt",
                radiusX = 8f,
                radiusZ = 8f,
                drawOnObject = true,
                delayDrawTime = 1000f,
                hitCounter = new HitCounter
                {
                    ActionID = new HashSet<uint> { 40284u }
                },
                distanceCheck = new DistanceCheck
                {
                    CheckObject = info.SourceId.GameObject(),
                    CheckType = 3
                }
            }, allPlayer);
        }
    }

    public override void OnAbilityCast(ActorAbilityInfo info)
    {
        if (info.ActionId != 40284)
        {
            return;
        }
        foreach (IGameObject allPlayer in PlayerHelper.AllPlayers)
        {
            DrawManager.Draw(new DrawElement
            {
                drawAvfx = "general_1bzt",
                radiusX = 8f,
                radiusZ = 8f,
                drawOnObject = true,
                hitCounter = new HitCounter
                {
                    ActionID = new HashSet<uint> { 40285u }
                },
                distanceCheck = new DistanceCheck
                {
                    CheckObject = info.Source,
                    CheckType = 2
                }
            }, allPlayer);
        }
    }
}
