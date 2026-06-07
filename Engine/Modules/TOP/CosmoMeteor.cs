using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;
using YapYapDraw.Engine.Util;

namespace YapYapDraw.Modules.TOP;

public class CosmoMeteor : ISpecialAction
{
    public override string Name => "Cosmo Meteor";

    public override uint Phase => 6u;

    public override uint WeatherID => 175u;

    public override HashSet<uint> ActionID => new HashSet<uint> { 31654u };

    public override void OnActionCast(ActorCastInfo info)
    {
        IGameObject checkObject = info.SourceId.GameObject();
        foreach (IGameObject allPlayer in PlayerHelper.AllPlayers)
        {
            DrawManager.Draw(new DrawElement
            {
                drawAvfx = "general_1bxf",
                radiusX = 8f,
                radiusZ = 8f,
                drawOnObject = true,
                delayDrawTime = 4000f,
                distanceCheck = new DistanceCheck
                {
                    CheckObject = checkObject,
                    CheckType = 2,
                    Count = 2
                },
                hitCounter = new HitCounter
                {
                    ActionID = new HashSet<uint> { 31655u }
                }
            }, allPlayer);
            DrawManager.Draw(new DrawElement
            {
                drawAvfx = "general_1bpxf",
                radiusX = 6f,
                radiusZ = 6f,
                drawOnObject = true,
                delayDrawTime = 4000f,
                distanceCheck = new DistanceCheck
                {
                    CheckObject = checkObject,
                    CheckType = 3
                },
                hitCounter = new HitCounter
                {
                    ActionID = new HashSet<uint> { 31656u }
                }
            }, allPlayer);
        }
    }
}
