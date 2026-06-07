using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;
using YapYapDraw.Engine.Util;

namespace YapYapDraw.Modules.FRU;

public class HouseOfLight : ISpecialAction
{
    public override string Name => "House of Light (cone bait)";

    public override uint Phase => 2u;

    public override HashSet<uint> ActionID => new HashSet<uint> { 40202u, 40203u };

    public override void OnActionCast(ActorCastInfo info)
    {
        foreach (IGameObject allPlayer in PlayerHelper.AllPlayers)
        {
            DrawManager.Draw(new DrawElement
            {
                drawAvfx = "gl_fan030_1bf",
                drawOnObject = true,
                radiusX = 60f,
                radiusZ = 60f,
                target = allPlayer,
                hitCounter = new HitCounter
                {
                    ActionID = new HashSet<uint> { 40206u }
                },
                distanceCheck = new DistanceCheck
                {
                    CheckObject = info.SourceId.GameObject(),
                    CheckType = 0,
                    Count = 4
                }
            }, info.SourceId.GameObject());
        }
    }
}
