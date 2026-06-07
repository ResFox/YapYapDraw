using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;
using YapYapDraw.Engine.Util;

namespace YapYapDraw.Modules.TEA;

public class Flarethrower : ISpecialAction
{
    public override string Name => "Flarethrower";

    public override uint Phase => 2u;

    public override HashSet<uint> ActionID => new HashSet<uint> { 18501u };

    public override void OnActionCast(ActorCastInfo info)
    {
        foreach (IGameObject allPlayer in PlayerHelper.AllPlayers)
        {
            DrawManager.Draw(new DrawElement
            {
                drawAvfx = "gl_fan090_1bf",
                radiusX = 100f,
                radiusZ = 100f,
                drawOnObject = true,
                target = allPlayer,
                distanceCheck = new DistanceCheck
                {
                    CheckObject = info.SourceId.GameObject(),
                    CheckType = 0
                },
                hitCounter = new HitCounter
                {
                    ActionID = new HashSet<uint> { 18502u }
                }
            }, info.SourceId.GameObject());
        }
    }
}
