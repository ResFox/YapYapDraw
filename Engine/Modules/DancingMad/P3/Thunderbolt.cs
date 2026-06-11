using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;
using YapYapDraw.Engine.Util;

namespace YapYapDraw.Modules.DancingMad.P3;

public class Thunderbolt : ISpecialAction
{
    public override string Name => "Thunderbolt";

    public override uint Phase => 3u;

    public override HashSet<uint> ActionID => new HashSet<uint> { 47890u, 47881u };

    public override void OnActionCast(ActorCastInfo info)
    {
        if (info.ActionId == 47890)
            SimpleElement.Circle(info);

        if (info.ActionId != 47881)
            return;

        foreach (IGameObject player in PlayerHelper.AllPlayers)
        {
            DrawManager.Draw(new DrawElement
            {
                drawAvfx = "general_1bzt",
                radiusX = 5f,
                radiusZ = 5f,
                distanceCheck = new DistanceCheck
                {
                    CheckType = 2,
                    CheckObject = info.SourceId.GameObject()
                },
                hitCounter = new HitCounter
                {
                    ActionID = new HashSet<uint> { 47884u },
                    TargetHitCount = 2
                }
            }, player);
        }
    }
}
