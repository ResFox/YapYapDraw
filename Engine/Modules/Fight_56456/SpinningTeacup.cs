using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using Dalamud.Game.ClientState.Objects.Types;
using YapYapDraw.Engine.Interop;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;
using YapYapDraw.Engine.Util;

namespace YapYapDraw.Modules.Fight_56456;

public class SpinningTeacup : ISpecialAction
{
    private static readonly List<IGameObject> Cups = new List<IGameObject>();

    private readonly Dictionary<uint, Action> cupPositions = new Dictionary<uint, Action>
    {
        {
            33554688u,
            () =>
            {
                int count = 5;
                List<(WPos, WPos?, WPos[])> configs = new List<(WPos, WPos?, WPos[])>(count);
                CollectionsMarshal.SetCount(configs, count);
                Span<(WPos, WPos?, WPos[])> span = CollectionsMarshal.AsSpan(configs);
                span[0] = (new WPos(17f, -163f), new WPos(17f, -177f), new WPos[2]
                {
                    new WPos(3.5f, -161.5f),
                    new WPos(30.5f, -178.5f)
                });
                span[1] = (new WPos(17f, -153f), new WPos(10f, -170f), new WPos[2]
                {
                    new WPos(25.5f, -156.5f),
                    new WPos(20.5f, -178.5f)
                });
                span[2] = (new WPos(17f, -153f), new WPos(17f, -177f), new WPos[2]
                {
                    new WPos(20.5f, -178.5f),
                    new WPos(3.5f, -161.5f)
                });
                span[3] = (new WPos(34f, -170f), null, new WPos[1]
                {
                    new WPos(8.5f, -173.5f)
                });
                span[4] = (new WPos(0f, -170f), null, new WPos[1]
                {
                    new WPos(25.5f, -166.5f)
                });
                HandleActivation(11.5f, configs);
            }
        },
        {
            268437504u,
            () =>
            {
                int count = 3;
                List<(WPos, WPos?, WPos[])> configs = new List<(WPos, WPos?, WPos[])>(count);
                CollectionsMarshal.SetCount(configs, count);
                Span<(WPos, WPos?, WPos[])> span = CollectionsMarshal.AsSpan(configs);
                span[0] = (new WPos(0f, -170f), new WPos(34f, -170f), new WPos[2]
                {
                    new WPos(8.5f, -156.5f),
                    new WPos(25.5f, -183.5f)
                });
                span[1] = (new WPos(0f, -170f), new WPos(17f, -187f), new WPos[2]
                {
                    new WPos(3.5f, -178.5f),
                    new WPos(8.5f, -156.5f)
                });
                span[2] = (new WPos(17f, -187f), new WPos(17f, -153f), new WPos[2]
                {
                    new WPos(30.5f, -161.5f),
                    new WPos(3.5f, -178.5f)
                });
                HandleActivation(14.5f, configs);
            }
        },
        {
            1048577u,
            () =>
            {
                AddAOEs(16f, new WPos(Cups[0].Position), new WPos(Cups[1].Position));
            }
        },
        {
            4194336u,
            () =>
            {
                int count = 2;
                List<(WPos, WPos?, WPos[])> configs = new List<(WPos, WPos?, WPos[])>(count);
                CollectionsMarshal.SetCount(configs, count);
                Span<(WPos, WPos?, WPos[])> span = CollectionsMarshal.AsSpan(configs);
                span[0] = (new WPos(0f, -170f), new WPos(17f, -163f), new WPos[2]
                {
                    new WPos(5f, -165f),
                    new WPos(22f, -182f)
                });
                span[1] = (new WPos(17f, -177f), new WPos(17f, -153f), new WPos[2]
                {
                    new WPos(5f, -175f),
                    new WPos(29f, -175f)
                });
                HandleActivation(19f, configs);
            }
        }
    };

    public override string Name => "Spinning Teacup";

    public override HashSet<uint> ActionID => new HashSet<uint> { 36720u };

    public override uint Phase => 2u;

    public override void OnAbilityCast(ActorAbilityInfo info)
    {
        Cups.Clear();
    }

    public override void OnActorTetherEvent(uint actorId, uint Id, ulong targetId)
    {
        if (Id == 276)
        {
            Cups.Add(actorId.GameObject());
        }
    }

    public override void OnEnvControl(byte index, uint state)
    {
        bool isCupIndex = index == 1 || index == 35;
        if (isCupIndex && cupPositions.TryGetValue(state, out Action handler))
        {
            handler();
        }
    }

    private static void HandleActivation(float time, List<(WPos pos1, WPos? pos2, WPos[] positions)> cups)
    {
        foreach (var (pos, pos2, positions) in cups)
        {
            if (CheckPositions(pos, pos2))
            {
                AddAOEs(time, positions);
                break;
            }
        }
    }

    private static void AddAOEs(float time, params WPos[] positions)
    {
        for (int i = 0; i < positions.Length; i++)
        {
            WPos wPos = positions[i];
            DrawManager.Draw(new DrawElement
            {
                drawAvfx = "general_1bxf",
                Position = new Vector3(wPos.X, ((IGameObject)Svc.Objects.LocalPlayer).Position.Y, wPos.Z),
                drawOnObject = false,
                radiusX = 19f,
                radiusZ = 19f,
                destroyTime = time * 1000f
            });
        }
    }

    private static bool CheckPositions(WPos pos1, WPos? pos2)
    {
        if (!pos2.HasValue)
        {
            return Cups.Any((IGameObject x) => new WPos(x.Position) == pos1);
        }
        if (Cups.Any((IGameObject x) => new WPos(x.Position) == pos1))
        {
            return Cups.Any((IGameObject x) =>
            {
                WPos cupPos = new WPos(x.Position);
                WPos? expected = pos2;
                return cupPos == expected;
            });
        }
        return false;
    }
}
