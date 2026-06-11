using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.ClientState.Objects.Types;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.Interop;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;
using YapYapDraw.Engine.Util;

namespace YapYapDraw.Modules.DancingMad.P3;

public class ChaosWater : ISpecialAction
{
    public override string Name => "Chaos Water";

    public override uint Phase => 3u;

    public override HashSet<uint> ActionID => new HashSet<uint> { 47862u };

    public override void OnAddStatus(ActorStatusChangeInfo info)
    {
        if (info.StatusID != 1601)
            return;

        IGameObject? target = info.TargetID.GameObject();
        if (target == null)
            return;

        DrawManager.Draw(new DrawElement
        {
            drawAvfx = "gl_donut1807_o0g",
            radiusX = 10f,
            radiusZ = 10f,
            destroyTime = 50000f,
            delayDrawTime = (info.Time - 5f) * 1000f,
            StatusCheck = new StatusCheck
            {
                CheckObject = target,
                Status = 1601u
            }
        }, target);
    }

    public override void OnAbilityCast(ActorAbilityInfo info)
    {
        if (NumCasts > 0)
            return;

        NumCasts++;
        IGameObject? source = Svc.Objects.FirstOrDefault(o => o.BaseId == 2015291);
        DrawElement element = new DrawElement
        {
            drawAvfx = "general_1bxf",
            radiusX = 5f,
            radiusZ = 5f,
            distanceCheck = new DistanceCheck
            {
                CheckObject = source,
                CheckType = 2,
                Count = 2
            },
            hitCounter = new HitCounter
            {
                ActionID = new HashSet<uint> { 47861u }
            }
        };
        foreach (IGameObject player in PlayerHelper.AllPlayers)
            DrawManager.Draw(element, player);
    }
}
