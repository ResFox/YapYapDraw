using System;
using System.Numerics;

namespace YapYapDraw.Engine.Helper;

public static class PositionHelper
{
    public static bool AlmostEqual(this Vector2 pos, Vector2 pos2, float eps)
        => (pos - pos2).AlmostZero(eps);

    public static bool AlmostEqual(this Vector3 pos, Vector3 pos2, float eps)
        => (pos - pos2).AlmostZero(eps);

    public static bool AlmostZero(this Vector2 a, float eps)
        => Math.Abs(a.X) <= eps && Math.Abs(a.Y) <= eps;

    public static bool AlmostZero(this Vector3 a, float eps)
        => Math.Abs(a.X) <= eps && Math.Abs(a.Z) <= eps;

    public static Vector2 RotationDegress(this Vector2 offset, float degrees, bool clockwise = false)
    {
        var rad = degrees * (MathF.PI / 180f);
        if (clockwise)
            return new Vector2(
                offset.X * MathF.Cos(rad) - offset.Y * MathF.Sin(rad),
                offset.X * MathF.Sin(rad) + offset.Y * MathF.Cos(rad));
        return new Vector2(
            offset.X * MathF.Cos(rad) + offset.Y * MathF.Sin(rad),
            -offset.X * MathF.Sin(rad) + offset.Y * MathF.Cos(rad));
    }

    public static Vector2 ToVector2(this Vector3 pos) => new(pos.X, pos.Z);

    public static bool IsPointInsideField(Vector3 center, Vector3 point, float rotation = 0f, float halfSide = 10f)
    {
        var cos = Math.Cos(-rotation);
        var sin = Math.Sin(-rotation);
        var x = (point.X - center.X) * cos - (point.Z - center.Z) * sin + center.X;
        var z = (point.X - center.X) * sin + (point.Z - center.Z) * cos + center.Z;
        return x >= center.X - halfSide && x <= center.X + halfSide
            && z >= center.Z - halfSide && z <= center.Z + halfSide;
    }
}
