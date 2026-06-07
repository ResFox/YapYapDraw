using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Enum;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;
using YapYapDraw.Engine.Util;

namespace YapYapDraw.Modules.M6S;

public class ColorRiot : ISpecialAction
{
    public override string Name => "Color Riot";

    public override HashSet<uint> ActionID => new HashSet<uint> { 42641u, 42642u };

    public override void OnActionCast(ActorCastInfo info)
    {
        foreach (IGameObject allPlayer in PlayerHelper.AllPlayers)
        {
            DrawElement element = new DrawElement
            {
                drawAvfx = "customCircle",
                radiusX = 4f,
                radiusZ = 4f,
                drawOnObject = true,
                refColor = ((info.ActionId == 42641) ? GroundOmen.Blue : GroundOmen.Red),
                distanceCheck = new DistanceCheck
                {
                    CheckObject = info.SourceId.GameObject(),
                    CheckType = 2
                },
                hitCounter = new HitCounter
                {
                    ActionID = new HashSet<uint> { 42643u, 42644u }
                }
            };
            DrawManager.Draw(element, allPlayer);
            element.refColor = ((info.ActionId == 42641) ? GroundOmen.Red : GroundOmen.Blue);
            element.distanceCheck = new DistanceCheck
            {
                CheckObject = info.SourceId.GameObject(),
                CheckType = 3
            };
            DrawManager.Draw(element, allPlayer);
        }
    }
}
