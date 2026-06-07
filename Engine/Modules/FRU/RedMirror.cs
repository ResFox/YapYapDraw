using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;
using YapYapDraw.Engine.Util;

namespace YapYapDraw.Modules.FRU;

public class RedMirror : ISpecialAction
{
    public override string Name => "Red Mirror";

    public override uint Phase => 2u;

    public override HashSet<uint> ActionID => new HashSet<uint> { 40205u };

    public override void OnActionCast(ActorCastInfo info)
    {
        SimpleElement.Donut(info, 4f, 20f);
        new TimeHelper(5000L, () =>
        {
            foreach (IGameObject player in PlayerHelper.AllPlayers)
            {
                DrawManager.Draw(new DrawElement
                {
                    drawAvfx = "gl_fan030_1bf",
                    drawOnObject = true,
                    radiusX = 60f,
                    radiusZ = 60f,
                    target = player,
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
        });
    }
}
