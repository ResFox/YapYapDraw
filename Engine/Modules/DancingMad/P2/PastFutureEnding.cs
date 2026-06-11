using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.Interop;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;
using YapYapDraw.Engine.Util;

namespace YapYapDraw.Modules.DancingMad.P2;

public class PastFutureEnding : ISpecialAction
{
    private bool _isFront;

    public override string Name => "Past / Future";

    public override uint Phase => 2u;

    public override HashSet<uint> ActionID => new HashSet<uint> { 47826u, 47827u, 47830u, 47831u, 47832u, 47833u, 47836u, 47837u };

    public override void OnActionCast(ActorCastInfo info)
    {
        if (info.ActionId != 47826 && info.ActionId != 47827)
            return;

        _isFront = info.ActionId == 47826;
        foreach (IGameObject player in PlayerHelper.AllPlayers)
        {
            if (player == Svc.Objects.LocalPlayer)
                continue;

            DrawManager.Draw(new DrawElement
            {
                drawAvfx = "general_1bxf",
                radiusX = 5f,
                radiusZ = 5f,
                distanceCheck = new DistanceCheck
                {
                    CheckType = 2,
                    CheckObject = info.SourceId.GameObject(),
                    Count = 4
                },
                hitCounter = new HitCounter
                {
                    ActionID = new HashSet<uint> { 47830u, 47831u, 47832u, 47833u }
                }
            }, player);
        }
    }

    public override void OnAbilityCast(ActorAbilityInfo info)
    {
        if (info.ActionId - 47830 <= 3)
        {
            if (NumCasts > 0)
                return;

            NumCasts++;
            DrawElement element = new DrawElement
            {
                drawAvfx = "gl_fan180_1bf",
                Position = info.Source.Position,
                drawOnObject = false,
                radiusX = 100f,
                radiusZ = 100f,
                target = Svc.Objects.LocalPlayer,
                refOffsetRotation = (_isFront ? 0.Degrees() : 180.Degrees()),
                hitCounter = new HitCounter
                {
                    ActionID = new HashSet<uint> { 47836u }
                }
            };
            aoes.Add(DrawManager.Draw(element));
        }

        if (info.ActionId - 47836 <= 1)
            NumCasts = 0;
    }
}
