using System;
using System.Collections.Generic;
using System.Numerics;
using Dalamud.Game.ClientState.Objects.Types;
using YapYapDraw.Engine.Interop;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.ModuleSetup;

namespace YapYapDraw.Modules.Fight_33687;

public class Castellation : ISpecialAction
{
    private static readonly Vector2[] Rects = new Vector2[5]
    {
        new Vector2(60f, 2f),
        new Vector2(60f, 4f),
        new Vector2(60f, 5f),
        new Vector2(60f, 6f),
        new Vector2(60f, 9f)
    };

    public override string Name => "Castellation";

    public override HashSet<uint> ActionID => new HashSet<uint>();

    private static HashSet<uint> CastEnd => new HashSet<uint> { 36614u, 36615u, 36616u, 36617u, 36618u };

    public override void OnEnvControl(byte index, uint state)
    {
        if (index == 3)
        {
            States(state, base.NumCasts != 1);
        }
    }

    private void States(uint state, bool check)
    {
        Dictionary<uint, (int, int)[]> layouts = new Dictionary<uint, (int, int)[]>();
        layouts[2097168u] = ((!check) ? new(int, int)[4]
        {
            (1, 116),
            (1, 94),
            (2, 105),
            (1, 84)
        } : new(int, int)[4]
        {
            (3, 114),
            (1, 90),
            (2, 101),
            (0, 82)
        });
        layouts[33554688u] = ((!check) ? Array.Empty<(int, int)>() : new(int, int)[3]
        {
            (2, 95),
            (4, 111),
            (1, 84)
        });
        layouts[134218752u] = ((!check) ? Array.Empty<(int, int)>() : new(int, int)[3]
        {
            (1, 116),
            (4, 101),
            (2, 85)
        });
        layouts[8388672u] = ((!check) ? new(int, int)[4]
        {
            (3, 114),
            (2, 101),
            (1, 90),
            (0, 82)
        } : new(int, int)[4]
        {
            (1, 116),
            (2, 105),
            (1, 94),
            (1, 84)
        });
        if (layouts.TryGetValue(state, out var layout))
        {
            (int, int)[] entries = layout;
            int i;
            for (i = 0; i < entries.Length; i++)
            {
                var (rectIndex, posX) = entries[i];
                DrawManager.Draw(new DrawElement
                {
                    drawAvfx = "general02wf",
                    Position = new Vector3(posX, 0f, 80f),
                    drawOnObject = false,
                    radiusX = Rects[rectIndex].Y,
                    radiusZ = Rects[rectIndex].X,
                    fixRotation = true,
                    hitCounter = new HitCounter
                    {
                        ActionID = CastEnd
                    }
                }, (IGameObject?)Svc.Objects.LocalPlayer);
            }
            i = base.NumCasts + 1;
            base.NumCasts = i;
        }
    }
}
