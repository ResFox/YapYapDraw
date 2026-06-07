using System;
using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.ClientState.Objects.Types;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.Memory;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;
using YapYapDraw.Engine.Util;

namespace YapYapDraw.Modules.P12S.P12S;

public class SuperchainTheory : ISpecialAction
{
    public override string Name => "Superchain Theory I";

    public override uint Phase => 1u;

    public override HashSet<uint> ActionID => new HashSet<uint>();

    public override void OnAddStatus(ActorStatusChangeInfo info)
    {
        if (info.StatusID != 2056 || info.Stack != 583)
        {
            return;
        }
        IEnumerable<TetherInfo> tethers = Data.TetherPlayer.Where((TetherInfo x) => x.To == info.TargetID);
        IGameObject target = info.TargetID.GameObject();
        foreach (TetherInfo tether in tethers)
        {
            float duration = (target.DistanceToTarget(tether.From.GameObject()) / 3f + 1f) * 1000f;
            switch (tether.TetherID)
            {
            case 228:
                SimpleElement.Circle(info.TargetID, 7f, Delay: Math.Max(duration - 4000f, 0f), CastTime: Math.Min(duration, 4000f));
                break;
            case 229:
                SimpleElement.Donut(info.TargetID, 6f, 70f, Delay: Math.Max(duration - 4000f, 0f), CastTime: Math.Min(duration, 4000f));
                break;
            case 230:
                foreach (IGameObject allPlayer in PlayerHelper.AllPlayers)
                {
                    IGameObject? source = info.TargetID.GameObject();
                    float delay = Math.Max(duration - 4000f, 0f);
                    float castTime = Math.Min(duration, 4000f);
                    SimpleElement.FanToTarget(source, allPlayer, 100f, 30, Follow: true, default, delay, castTime);
                }
                break;
            case 231:
                foreach (IGameObject member in PlayerHelper.Tank.Union(PlayerHelper.Healer))
                {
                    DrawManager.Draw(new DrawElement
                    {
                        drawAvfx = "gl_fan030_1bpf",
                        radiusX = 100f,
                        radiusZ = 100f,
                        drawOnObject = true,
                        target = member,
                        destroyTime = Math.Min(duration, 4000f),
                        delayDrawTime = Math.Max(duration - 4000f, 0f)
                    }, info.TargetID.GameObject());
                }
                break;
            }
        }
    }
}
