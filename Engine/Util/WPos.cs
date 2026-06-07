using System;
using System.Numerics;

namespace YapYapDraw.Engine.Util;

public readonly struct WPos(float x, float z)
{
    public readonly float X = x;
    public readonly float Z = z;

    public WPos(Vector2 v) : this(v.X, v.Y) { }
    public WPos(Vector3 v) : this(v.X, v.Z) { }

    public Vector2 ToVec2() => new(X, Z);
    public Vector3 ToVec3(float y = 0f) => new(X, y, Z);
    public Vector4 ToVec4(float y = 0f, float w = 0f) => new(X, y, Z, w);
    public WDir ToWDir() => new(X, Z);

    public static bool operator ==(WPos left, WPos right) => left.X == right.X && left.Z == right.Z;
    public static bool operator !=(WPos left, WPos right) => !(left == right);

    public static WPos operator *(WPos a, float b) => new(a.X * b, a.Z * b);
    public static WPos operator +(WPos a, float b) => new(a.X + b, a.Z + b);

    public static WPos operator /(WPos a, int b)
    {
        float inv = 1f / b;
        return new WPos(a.X * inv, a.Z * inv);
    }

    public static WPos operator /(WPos a, float b)
    {
        float inv = 1f / b;
        return new WPos(a.X * inv, a.Z * inv);
    }

    public static WPos operator +(WPos a, WDir b) => new(a.X + b.X, a.Z + b.Z);
    public static WPos operator +(WDir a, WPos b) => new(a.X + b.X, a.Z + b.Z);
    public static WPos operator -(WPos a, WDir b) => new(a.X - b.X, a.Z - b.Z);
    public static WDir operator -(WPos a, WPos b) => new(a.X - b.X, a.Z - b.Z);

    public bool AlmostEqual(WPos b, float eps) => Math.Abs(X - b.X) <= eps && Math.Abs(Z - b.Z) <= eps;

    public WPos Scaled(float multiplier) => new(X * multiplier, Z * multiplier);
    public WPos Rounded() => new(MathF.Round(X), MathF.Round(Z));
    public WPos Rounded(float precision) => Scaled(1f / precision).Rounded().Scaled(precision);

    public static WPos Lerp(WPos from, WPos to, float progress) =>
        new(from.ToVec2() * (1f - progress) + to.ToVec2() * progress);

    public WPos Quantized() => new(
        ((int)MathF.Round(X * 32.7675f) - 0.5f) * 0.030518044f,
        ((int)MathF.Round(Z * 32.7675f) - 0.5f) * 0.030518044f);

    public static WPos RotateAroundOrigin(float rotateByDegrees, WPos origin, WPos point)
    {
        var (sinD, cosD) = Math.SinCos(rotateByDegrees * (MathF.PI / 180f));
        float sin = (float)sinD;
        float cos = (float)cosD;
        float dx = point.X - origin.X;
        float dz = point.Z - origin.Z;
        float rx = cos * dx - sin * dz;
        float rz = sin * dx + cos * dz;
        return new WPos(origin.X + rx, origin.Z + rz);
    }

    public static WPos[] GenerateRotatedVertices(WPos center, WPos[] vertices, float rotationAngle)
    {
        var result = new WPos[vertices.Length];
        for (int i = 0; i < vertices.Length; i++)
            result[i] = RotateAroundOrigin(rotationAngle, center, vertices[i]);
        return result;
    }

    public override string ToString() => $"[{X:f3}, {Z:f3}]";

    public bool Equals(WPos other) => this == other;
    public override bool Equals(object? obj) => obj is WPos other && Equals(other);
    public override int GetHashCode() => (X, Z).GetHashCode();

    public bool InTri(WPos v1, WPos v2, WPos v3)
    {
        float d1 = (v2.X - v1.X) * (Z - v1.Z) - (v2.Z - v1.Z) * (X - v1.X);
        float d2 = (v3.X - v2.X) * (Z - v2.Z) - (v3.Z - v2.Z) * (X - v2.X);
        if (d1 < 0f != d2 < 0f && d1 != 0f && d2 != 0f)
            return false;
        float d3 = (v1.X - v3.X) * (Z - v3.Z) - (v1.Z - v3.Z) * (X - v3.X);
        if (d3 != 0f)
            return d3 < 0f == d1 + d2 <= 0f;
        return true;
    }

    public bool InRect(WPos origin, WDir direction, float lenFront, float lenBack, float halfWidth) =>
        (this - origin).InRect(direction, lenFront, lenBack, halfWidth);

    public bool InRect(WPos origin, Angle direction, float lenFront, float lenBack, float halfWidth) =>
        (this - origin).InRect(direction.ToDirection(), lenFront, lenBack, halfWidth);

    public bool InRect(WPos origin, WDir startToEnd, float halfWidth)
    {
        float len = startToEnd.Length();
        return InRect(origin, startToEnd / len, len, 0f, halfWidth);
    }

    public bool InRect(WPos origin, WPos end, float halfWidth) => InRect(origin, end - origin, halfWidth);

    public bool InSquare(WPos origin, float halfWidth, Angle rotation) =>
        (this - origin).InRect(rotation.ToDirection(), halfWidth, halfWidth, halfWidth);

    public bool InSquare(WPos origin, float halfWidth, WDir rotation) =>
        (this - origin).InRect(rotation, halfWidth, halfWidth, halfWidth);

    public bool InSquare(WPos origin, float halfWidth) =>
        Math.Abs(X - origin.X) <= halfWidth && Math.Abs(Z - origin.Z) <= halfWidth;

    public bool InRect(WPos origin, float halfWidth, float halfHeight) =>
        Math.Abs(X - origin.X) <= halfWidth && Math.Abs(Z - origin.Z) <= halfHeight;

    public bool InCross(WPos origin, Angle direction, float length, float halfWidth) =>
        (this - origin).InCross(direction.ToDirection(), length, halfWidth);

    public bool InCross(WPos origin, WDir direction, float length, float halfWidth) =>
        (this - origin).InCross(direction, length, halfWidth);

    public bool InCircle(WPos origin, float radius) => (this - origin).LengthSq() <= radius * radius;

    public bool InDonut(WPos origin, float innerRadius, float outerRadius) =>
        InCircle(origin, outerRadius) && !InCircle(origin, innerRadius);

    public bool InCone(WPos origin, WDir direction, Angle halfAngle) =>
        (this - origin).Normalized().Dot(direction) >= halfAngle.Cos();

    public bool InCone(WPos origin, Angle direction, Angle halfAngle) =>
        InCone(origin, direction.ToDirection(), halfAngle);

    public bool InCircleCone(WPos origin, float radius, WDir direction, Angle halfAngle) =>
        InCircle(origin, radius) && InCone(origin, direction, halfAngle);

    public bool InCircleCone(WPos origin, float radius, Angle direction, Angle halfAngle) =>
        InCircle(origin, radius) && InCone(origin, direction, halfAngle);

    public bool InDonutCone(WPos origin, float innerRadius, float outerRadius, WDir direction, Angle halfAngle) =>
        InDonut(origin, innerRadius, outerRadius) && InCone(origin, direction, halfAngle);

    public bool InDonutCone(WPos origin, float innerRadius, float outerRadius, Angle direction, Angle halfAngle) =>
        InDonut(origin, innerRadius, outerRadius) && InCone(origin, direction, halfAngle);

    public bool InCapsule(WPos origin, WDir direction, float radius, float length)
    {
        float t = Math.Clamp((this - origin).Dot(direction), 0f, length);
        WPos closest = origin + t * direction;
        return (this - closest).LengthSq() <= radius * radius;
    }

    public bool InArcCapsule(WPos start, WDir toOrbitCenter, Angle angularLength, float tubeRadius) =>
        InArcCapsule(start, start + toOrbitCenter, angularLength, tubeRadius);

    public bool InArcCapsule(WPos start, WPos orbitCenter, Angle angularLength, float tubeRadius)
    {
        float tubeRadiusSq = tubeRadius * tubeRadius;
        float x = orbitCenter.X;
        float z = orbitCenter.Z;
        WDir dir = new(start.X - x, start.Z - z);
        float orbitRadius = dir.Length();
        WDir rotated = dir.Rotate(angularLength);
        WPos arcEnd = new(x + rotated.X, z + rotated.Z);

        if ((this - start).LengthSq() <= tubeRadiusSq)
            return true;
        if ((this - arcEnd).LengthSq() <= tubeRadiusSq)
            return true;

        float dx = X - x;
        float dz = Z - z;
        float distSq = dx * dx + dz * dz;
        float innerBound = orbitRadius - tubeRadius;
        float outerBound = orbitRadius + tubeRadius;
        if (distSq < innerBound * innerBound || distSq > outerBound * outerBound)
            return false;

        Angle halfSpan = angularLength.Abs() * 0.5f;
        float sign = halfSpan.Rad > MathF.PI / 2f ? -1f : 1f;
        Angle mid = Angle.FromDirection(dir) + angularLength * 0.5f;
        Angle right = 90f.Degrees();
        WDir edge1 = sign * (mid + halfSpan + right).ToDirection();
        WDir edge2 = sign * (mid - halfSpan - right).ToDirection();
        float dot1 = dx * edge1.X + dz * edge1.Z;
        float dot2 = dx * edge2.X + dz * edge2.Z;

        if (sign > 0f)
            return dot1 <= 0f && dot2 <= 0f;
        return dot1 >= 0f || dot2 >= 0f;
    }
}
