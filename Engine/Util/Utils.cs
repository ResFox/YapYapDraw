using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;

namespace YapYapDraw.Engine.Util;

public static class Utils
{
    public struct GridSnapper
    {
        public Vector3 Center;
        public float Size;
        public int GridCount;

        public readonly Vector3 Snap(Vector3 pos) => SnapToGrid(pos, Center, Size, GridCount);
    }

    public static void RotateList<T>(List<T> list, int startIndex)
    {
        var count = list.Count;
        if (count <= 1 || startIndex == 0 || startIndex % count == 0) return;
        startIndex %= count;
        Reverse(list, 0, startIndex - 1);
        Reverse(list, startIndex, count - 1);
        Reverse(list, 0, count - 1);
    }

    public static Vector3 SnapToGrid(Vector3 pos, Vector3 center, float size, int gridCount)
    {
        var cell = size / gridCount;
        var half = size / 2f;
        return new Vector3(SnapAxis(pos.X, center.X, half, cell, gridCount), pos.Y, SnapAxis(pos.Z, center.Z, half, cell, gridCount));
    }

    private static void Reverse<T>(List<T> list, int start, int end)
    {
        var span = CollectionsMarshal.AsSpan(list);
        while (start < end)
        {
            (span[start], span[end]) = (span[end], span[start]);
            start++;
            end--;
        }
    }

    private static float SnapAxis(float value, float center, float half, float cell, int gridCount)
    {
        var index = (int)MathF.Floor((value - (center - half)) / cell);
        index = Math.Clamp(index, 0, gridCount - 1);
        return center - half + index * cell + cell / 2f;
    }
}
