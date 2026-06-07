using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;
using YapYapDraw.Engine.Util;

namespace YapYapDraw.Modules.M11S;

public class TwinSpinFire : ISpecialAction
{
    public override string Name => "Twin Spin Fire";

    public override HashSet<uint> ActionID => new HashSet<uint> { 47037u };

    public override void OnActionCast(ActorCastInfo info)
    {
        foreach (IGameObject allPlayer in PlayerHelper.AllPlayers)
        {
            DrawManager.Draw(new DrawElement
            {
                drawAvfx = "general02xf",
                target = allPlayer,
                radiusZ = 60f,
                radiusX = 3f,
                distanceCheck = new DistanceCheck
                {
                    CheckType = 0,
                    Count = 2,
                    CheckObject = info.SourceId.GameObject()
                },
                hitCounter = new HitCounter
                {
                    ActionID = new HashSet<uint> { 47038u }
                }
            }, info.SourceId.GameObject());
        }
    }
}
