using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;
using YapYapDraw.Engine.Util;

namespace YapYapDraw.Modules.M5S;

public class BackUpDance : ISpecialAction
{
    public override string Name => "Back-up Dance";

    public override HashSet<uint> ActionID => new HashSet<uint> { 42871u };

    public override void OnActionCast(ActorCastInfo info)
    {
        foreach (IGameObject allPlayer in PlayerHelper.AllPlayers)
        {
            DrawManager.Draw(new DrawElement
            {
                drawAvfx = "gl_fan045_1bf",
                radiusX = 60f,
                radiusZ = 60f,
                drawOnObject = true,
                target = allPlayer,
                hitCounter = new HitCounter
                {
                    ActionID = new HashSet<uint> { 42872u }
                },
                distanceCheck = new DistanceCheck
                {
                    CheckObject = info.SourceId.GameObject(),
                    CheckType = 0
                }
            }, info.SourceId.GameObject());
        }
    }
}
