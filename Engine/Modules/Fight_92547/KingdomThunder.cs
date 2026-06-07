using System;
using System.Collections.Generic;
using System.Numerics;
using Dalamud.Game.ClientState.Objects.Types;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;
using YapYapDraw.Engine.Util;
using YapYapDraw.Engine.Vfx;

namespace YapYapDraw.Modules.Fight_92547;

public class KingdomThunder : ISpecialAction
{
    private bool[] activeTiles = new bool[6];

    private readonly Dictionary<ulong, List<StaticVfx>> actorVfx = new Dictionary<ulong, List<StaticVfx>>();

    public override string Name => "Kingdom Thunder";

    public override HashSet<uint> ActionID => new HashSet<uint> { 43102u, 43439u, 43446u, 43447u };

    public override void OnEnvControl(byte index, uint state)
    {
        if (index >= 4 && index <= 9)
        {
            switch (state)
            {
            case 4194560u:
                activeTiles[index - 4] = true;
                break;
            case 262176u:
                activeTiles[index - 4] = false;
                break;
            }
        }
    }

    public override void OnActionCast(ActorCastInfo info)
    {
        ushort actionId = info.ActionId;
        if (actionId == 43102 || actionId == 43439)
        {
            Span<bool> span = stackalloc bool[6];
            WPos coneCenter = new WPos(100f, 100f);
            WPos circleCenter = new WPos(info.Pos);
            Angle halfAngle = 30f.Degrees();
            uint sourceId = info.SourceId;
            StaticVfx vfx = SimpleElement.Circle(info.Pos, 4f, Environment.TickCount64);
            aoes.Add(vfx);
            if (!actorVfx.TryGetValue(sourceId, out List<StaticVfx> vfxList))
            {
                actorVfx[sourceId] = new List<StaticVfx>();
            }
            actorVfx[sourceId].Add(vfx);
            for (int i = 0; i < 6; i++)
            {
                if (!activeTiles[i] || span[i] || !Intersect.CircleCone(circleCenter, 4f, coneCenter, 16f, (180f - 60f * (float)i).Degrees().ToDirection(), halfAngle))
                {
                    continue;
                }
                int left = i;
                while (true)
                {
                    int prev = (left - 1 + 6) % 6;
                    if (!activeTiles[prev] || span[prev])
                    {
                        break;
                    }
                    left = prev;
                }
                int right = i;
                while (true)
                {
                    int next = (right + 1) % 6;
                    if (!activeTiles[next] || span[next])
                    {
                        break;
                    }
                    right = next;
                }
                int current = left;
                while (true)
                {
                    if (!span[current])
                    {
                        span[current] = true;
                        vfx = SimpleElement.Fan(new Vector3(100f, 0f, 100f), 16f, 60, (180f - 60f * (float)current).Degrees(), Environment.TickCount64);
                        aoes.Add(vfx);
                        if (!actorVfx.TryGetValue(sourceId, out vfxList))
                        {
                            actorVfx[sourceId] = new List<StaticVfx>();
                        }
                        actorVfx[sourceId].Add(vfx);
                    }
                    if (current != right)
                    {
                        current = (current + 1) % 6;
                        continue;
                    }
                    break;
                }
                break;
            }
            return;
        }
        actionId = info.ActionId;
        bool isFanCast = (uint)(actionId - 43446) <= 1u;
        if (!isFanCast || aoes.Count != 0)
        {
            return;
        }
        for (int j = 0; j < 6; j++)
        {
            if (activeTiles[j])
            {
                SimpleElement.Fan(new Vector3(100f, 0f, 100f), 16f, 60, (180f - 60f * (float)j).Degrees(), info.CastTime * 1000f);
            }
        }
    }

    public override void OnActorPlayActionTimelineEvent(IGameObject source, uint id)
    {
        if (id == 7750)
        {
            int degrees = (int)source.Rotation.Radians().Deg;
            int tile = -1;
            switch (degrees)
            {
            case -143:
            case 143:
            case 180:
                tile = 0;
                break;
            case 83:
            case 119:
            case 156:
                tile = 1;
                break;
            case 23:
            case 59:
            case 96:
                tile = 2;
                break;
            case -36:
            case 0:
            case 36:
                tile = 3;
                break;
            case -96:
            case -60:
            case -23:
                tile = 4;
                break;
            case -156:
            case -120:
            case -83:
                tile = 5;
                break;
            }
            if (tile >= 0)
            {
                activeTiles[tile] = true;
            }
        }
    }

    public override void OnAbilityCast(ActorAbilityInfo info)
    {
        uint actionId = info.ActionId;
        if ((actionId != 43102 && actionId != 43439) || !actorVfx.TryGetValue(info.Source.GameObjectId, out List<StaticVfx> vfxList))
        {
            return;
        }
        foreach (StaticVfx vfx in vfxList)
        {
            aoes.Remove(vfx);
            vfx.Remove();
        }
        actorVfx.Remove(info.Source.GameObjectId);
    }

    public override void Reset()
    {
        activeTiles = new bool[6];
        actorVfx.Clear();
        base.Reset();
    }
}
