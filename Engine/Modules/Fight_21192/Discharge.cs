using System.Collections.Generic;
using System.Numerics;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.ModuleSetup;

namespace YapYapDraw.Modules.Fight_21192;

public class Discharge : ISpecialAction
{
    private readonly Dictionary<byte, Vector2> initialPositions = new Dictionary<byte, Vector2>
    {
        {
            13,
            new Vector2(81.132f, 268.868f)
        },
        {
            14,
            new Vector2(81.132f, 285.132f)
        },
        {
            15,
            new Vector2(64.868f, 268.868f)
        },
        {
            16,
            new Vector2(64.868f, 285.132f)
        }
    };

    private readonly Dictionary<byte, byte> pairsWithSamePositions = new Dictionary<byte, byte>
    {
        { 13, 16 },
        { 14, 15 },
        { 15, 14 },
        { 16, 13 }
    };

    public override string Name => "Discharge";

    public override HashSet<uint> ActionID => new HashSet<uint>();

    public override void OnEnvControl(byte index, uint state)
    {
        if (state == 131073 || state == 2097168)
        {
            if (state == 2097168 && pairsWithSamePositions.TryGetValue(index, out var pairedIndex))
            {
                index = pairedIndex;
            }
            if (initialPositions.TryGetValue(index, out var position))
            {
                SimpleElement.Circle(new Vector3(position.X, -0.8f, position.Y), 26f, 3000f, 0f, new HitCounter
                {
                    ActionID = new HashSet<uint> { 40626u }
                });
            }
        }
    }
}
