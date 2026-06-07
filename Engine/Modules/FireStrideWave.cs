using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.ClientState.Objects.Types;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;
using YapYapDraw.Engine.Util;

namespace YapYapDraw.Modules.M10S;

public class FireStrideWave : ISpecialAction
{
    public override string Name => "Fire Stride Wave";

    public override HashSet<uint> ActionID => new HashSet<uint> { 46532u };

    public override void OnActionCast(ActorCastInfo info)
    {
        foreach (IGameObject item in PlayerHelper.AllPlayers.Where((IGameObject x) => x.HasStatus(4974u)))
        {
            DrawManager.Draw(new DrawElement
            {
                drawAvfx = "general_1bzt",
                drawOnObject = true,
                radiusX = 6f,
                radiusZ = 6f,
                distanceCheck = new DistanceCheck
                {
                    CheckType = 3,
                    Count = 1,
                    CheckObject = info.SourceId.GameObject()
                },
                hitCounter = new HitCounter
                {
                    ActionID = new HashSet<uint> { 47390u, 47391u, 47392u, 47393u },
                    TargetHitCount = 4
                }
            }, item);
        }
    }
}
