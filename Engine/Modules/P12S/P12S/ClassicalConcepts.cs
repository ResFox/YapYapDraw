using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;
using Dalamud.Game.ClientState.Objects.Types;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.ModuleSetup;

namespace YapYapDraw.Modules.P12S.P12S;

public class ClassicalConcepts : ISpecialAction
{
    private int cubeCount;

    private int[,] cube = new int[4, 3];

    private IGameObject[,] cubeGameObject = new IGameObject[4, 3];

    private List<(int, int)> bias;

    public override string Name => "Classical Concepts";

    public override uint Phase => 2u;

    public override HashSet<uint> ActionID => new HashSet<uint>();

    private void DrawLines(bool swap)
    {
        if (swap)
        {
            for (int i = 0; i < 4; i++)
            {
                ref int first = ref cube[i, 0];
                ref int last = ref cube[3 - i, 2];
                int lastVal = cube[3 - i, 2];
                int firstVal = cube[i, 0];
                first = lastVal;
                last = firstVal;
                if (i < 2)
                {
                    first = ref cube[i, 1];
                    ref int mid = ref cube[3 - i, 1];
                    firstVal = cube[3 - i, 1];
                    lastVal = cube[i, 1];
                    first = firstVal;
                    mid = lastVal;
                }
            }
        }
        for (int j = 0; j < 3; j++)
        {
            Plugin.DebugLog($"{cube[0, j]}, {cube[1, j]}, {cube[2, j]}, {cube[3, j]}");
        }
        for (int k = 0; k < 4; k++)
        {
            for (int l = 0; l < 3; l++)
            {
                if (cube[k, l] != 2)
                {
                    continue;
                }
                List<(int, int)> ones = new List<(int, int)>();
                List<(int, int)> threes = new List<(int, int)>();
                foreach (var offset in bias)
                {
                    int nx = k + offset.Item1;
                    int ny = l + offset.Item2;
                    if (nx >= 0 && nx <= 3 && ny >= 0 && ny <= 2)
                    {
                        if (cube[nx, ny] == 1)
                        {
                            ones.Add((nx, ny));
                        }
                        if (cube[nx, ny] == 3)
                        {
                            threes.Add((nx, ny));
                        }
                    }
                }
                if (ones.Count == 1)
                {
                    DrawLine(k, l, ones[0].Item1, ones[0].Item2, isRed: true, swap);
                }
                else
                {
                    int twoCount = 0;
                    foreach (var offset in bias)
                    {
                        int nx = ones[0].Item1 + offset.Item1;
                        int ny = ones[0].Item2 + offset.Item2;
                        if (nx >= 0 && nx <= 3 && ny >= 0 && ny <= 2 && cube[nx, ny] == 2)
                        {
                            twoCount++;
                        }
                    }
                    int pick = ((twoCount == 2) ? 1 : 0);
                    DrawLine(k, l, ones[pick].Item1, ones[pick].Item2, isRed: true, swap);
                }
                if (threes.Count == 1)
                {
                    DrawLine(k, l, threes[0].Item1, threes[0].Item2, isRed: false, swap);
                    continue;
                }
                int twoCount2 = 0;
                foreach (var offset in bias)
                {
                    int nx = threes[0].Item1 + offset.Item1;
                    int ny = threes[0].Item2 + offset.Item2;
                    if (nx >= 0 && nx <= 3 && ny >= 0 && ny <= 2 && cube[nx, ny] == 2)
                    {
                        twoCount2++;
                    }
                }
                int pick2 = ((twoCount2 == 2) ? 1 : 0);
                DrawLine(k, l, threes[pick2].Item1, threes[pick2].Item2, isRed: false, swap);
            }
        }
    }

    private void DrawLine(int x1, int y1, int x2, int y2, bool isRed, bool swap)
    {
        Plugin.DebugLog($"draw: ({x1}, {y1}) => ({x2}, {y2})");
        DrawManager.Draw(new DrawElement
        {
            drawAvfx = (isRed ? "general02xf" : "general02pxf"),
            radiusX = 1f,
            radiusY = 1f,
            target = cubeGameObject[x2, y2],
            drawOnObject = true,
            endToTarget = true,
            delayDrawTime = (swap ? 5000 : 0),
            hitCounter = new HitCounter
            {
                ActionID = (swap ? new HashSet<uint> { 33591u } : new HashSet<uint> { 33587u })
            }
        }, cubeGameObject[x1, y1]);
    }

    public override void OnObjectCreatedEvent(IGameObject gameObject)
    {
        if ((gameObject != null && gameObject.BaseId == 16183) || (gameObject != null && gameObject.BaseId == 16184) || (gameObject != null && gameObject.BaseId == 16185))
        {
            string color = ((gameObject.BaseId == 16183) ? "red" : ((gameObject.BaseId == 16184) ? "blue" : "yellow"));
            Vector2 pos = gameObject.Position.ToVector2();
            Plugin.DebugLog($"cube color:{color}, position:{pos}");
            int col = ((int)pos.X - 88) / 8;
            int row = ((int)pos.Y - 84) / 8;
            cube[col, row] = (int)(gameObject.BaseId - 16182);
            cubeGameObject[col, row] = gameObject;
            cubeCount++;
            if (cubeCount == 12)
            {
                DrawLines(swap: false);
            }
            if (cubeCount == 24)
            {
                DrawLines(swap: true);
            }
        }
    }

    public override void Reset()
    {
        cubeCount = 0;
        base.Reset();
    }

    public ClassicalConcepts()
    {
        bias = new List<(int, int)> { (1, 0), (-1, 0), (0, 1), (0, -1) };
    }
}
