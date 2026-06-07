using System.Collections.Generic;
using System.Numerics;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.Memory;

namespace YapYapDraw.Engine;

public static class Data
{
    public static HashSet<string> lockonList = new();

    public static HashSet<string> omenList = new()
    {
        "customFan",
        "customCircle",
        "customDonut",
        "customRect",
        "customRect2",
        "share2_6m",
        "eye_warn",
        "tank_lockon_3m_5s_noc",
        "tank_lockon_5m_5s_noc",
        "ShareLazerGround5s"
    };

    public static HashSet<string> channelingList = new();

    public static List<TimeHelper> DelayTasks { get; set; } = new();

    public static List<TetherInfo> TetherPlayer { get; set; } = new();

    public static Dictionary<ulong, Vector3> LastCastPositions = new();

    public static readonly Actor?[] ActorsByIndex = new Actor[819];

    public static readonly Dictionary<ulong, Actor> Actors = new();

    public static void Clear()
    {
        TetherPlayer.Clear();
    }
}
