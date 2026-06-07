using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Game.ClientState.Objects.Types;
using YapYapDraw.Engine.Interop;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;
using YapYapDraw.Engine.Util;
using YapYapDraw.Engine.Vfx;

namespace YapYapDraw.Modules.E3;

public class NightRaidCommand24Portals : ISpecialAction
{
    public int DoorIndex;

    public int FirstStatus;

    public HashSet<int> DoorIndexs = new HashSet<int>();

    public Dictionary<int, (string first, string second)> DoorMap = new Dictionary<int, (string, string)>
    {
        {
            11,
            ("Red", "Blue")
        },
        {
            12,
            ("Blue", "Red")
        }
    };

    public Dictionary<string, int[]> refZPatterns = new Dictionary<string, int[]>
    {
        {
            "Red_12",
            new int[2] { 95, 115 }
        },
        {
            "Red_34",
            new int[2] { 85, 105 }
        },
        {
            "Red_67",
            new int[2] { 85, 105 }
        },
        {
            "Red_89",
            new int[2] { 95, 115 }
        },
        {
            "Blue_12",
            new int[2] { 85, 105 }
        },
        {
            "Blue_34",
            new int[2] { 95, 115 }
        },
        {
            "Blue_67",
            new int[2] { 95, 115 }
        },
        {
            "Blue_89",
            new int[2] { 85, 105 }
        }
    };

    public override string Name => "Night Raid Command (2+4 portals)";

    public override HashSet<uint> ActionID => new HashSet<uint> { 19516u, 19517u, 19518u, 19521u };

    public override IEnumerable<StaticVfx> ActiveAOEs => aoes.Take(2);

    public override void OnEnvControl(byte index, uint state)
    {
        if (state == 131073)
        {
            if ((uint)(index - 11) <= 1u)
            {
                DoorIndex = index;
            }
            if ((uint)(index - 1) <= 3u || (uint)(index - 6) <= 3u)
            {
                DoorIndexs.Add(index);
            }
        }
        if (state == 524292)
        {
            DoorIndex = 0;
            DoorIndexs.Clear();
        }
    }

    public override void OnActionCast(ActorCastInfo info)
    {
        if (((IGameObject?)Svc.Objects.LocalPlayer).HasStatus(2238u))
        {
            FirstStatus = 2238;
        }
        if (((IGameObject?)Svc.Objects.LocalPlayer).HasStatus(2239u))
        {
            FirstStatus = 2239;
        }
    }

    public override void OnAbilityCast(ActorAbilityInfo info)
    {
        uint actionId = info.ActionId;
        bool firstSetAction = (actionId == 19516 || actionId == 19518);
        bool matched = firstSetAction && FirstStatus == 2238;
        if (!matched)
        {
            uint actionId2 = info.ActionId;
            bool secondSetAction = (actionId2 == 19517 || actionId2 == 19521);
            matched = secondSetAction && FirstStatus == 2239;
        }
        if (!matched || !DoorMap.TryGetValue(DoorIndex, out (string, string) value) || info.Source.Position.X < 90f || info.Source.Position.X > 110f)
        {
            return;
        }
        DrawElement drawElement = new DrawElement
        {
            Enable = false,
            drawAvfx = "general02xf",
            drawOnObject = false,
            radiusX = 5f,
            radiusZ = 100f,
            refRotation = -90.Degrees(),
            fixRotation = true,
            hitCounter = new HitCounter
            {
                ActionID = new HashSet<uint> { 19520u }
            }
        };
        string value2 = info.Source.Position.X < 100f ? value.Item1 : value.Item2;
        string key = $"{value2}_{DoorIndexs.First()}{DoorIndexs.Last()}";
        if (refZPatterns.TryGetValue(key, out int[] value3))
        {
            int[] zCoords = value3;
            foreach (int z in zCoords)
            {
                drawElement.Position = new Vector3(120f, 0f, z);
                aoes.Add(DrawManager.Draw(drawElement));
            }
        }
        DoorIndex = 0;
        DoorIndexs.Clear();
    }

    public override void Reset()
    {
        FirstStatus = 0;
        DoorIndex = 0;
        DoorIndexs.Clear();
        base.Reset();
    }
}
