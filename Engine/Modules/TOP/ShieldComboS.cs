using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;
using YapYapDraw.Engine.Util;

namespace YapYapDraw.Modules.TOP;

public class ShieldComboS : ISpecialAction
{
    public override string Name => "Shield Combo S";

    public override uint Phase => 2u;

    public override uint WeatherID => 78u;

    public override HashSet<uint> ActionID => new HashSet<uint> { 31527u };

    public override void OnActionCast(ActorCastInfo info)
    {
        foreach (IGameObject allPlayer in PlayerHelper.AllPlayers)
        {
            DrawManager.Draw(new DrawElement
            {
                drawAvfx = "general_1bxf",
                radiusX = 5f,
                radiusZ = 5f,
                drawOnObject = true,
                distanceCheck = new DistanceCheck
                {
                    CheckObject = info.SourceId.GameObject(),
                    CheckType = 2,
                    Count = 2
                },
                hitCounter = new HitCounter
                {
                    ActionID = new HashSet<uint> { 31528u }
                }
            }, allPlayer);
        }
    }
}
