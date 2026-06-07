using System;
using System.Numerics;

namespace YapYapDraw.Engine.Util;

public readonly struct WDir(float x, float z)
{
    public readonly float X = x;
    public readonly float Z = z;

    public WDir(Vector2 v) : this(v.X, v.Y) { }

    public Vector2 ToVec2() => new(X, Z);
    public Vector3 ToVec3(float y = 0f) => new(X, y, Z);
    public Vector4 ToVec4(float y = 0f, float w = 0f) => new(X, y, Z, w);
    public WPos ToWPos() => new(X, Z);

    public static bool operator ==(WDir left, WDir right) => left.X == right.X && left.Z == right.Z;
    public static bool operator !=(WDir left, WDir right) => !(left == right);

    public static WDir operator +(WDir a, WDir b) => new(a.X + b.X, a.Z + b.Z);
    public static WDir operator -(WDir a, WDir b) => new(a.X - b.X, a.Z - b.Z);
    public static WDir operator -(WDir a) => new(-a.X, -a.Z);
    public static WDir operator -(WDir a, WPos b) => new(a.X - b.X, a.Z - b.Z);
    public static WDir operator *(WDir a, float b) => new(a.X * b, a.Z * b);
    public static WDir operator *(float a, WDir b) => new(a * b.X, a * b.Z);

    public static WDir operator /(WDir a, float b)
    {
        float inv = 1f / b;
        return new WDir(a.X * inv, a.Z * inv);
    }

    public WDir Abs() => new(Math.Abs(X), Math.Abs(Z));
    public WDir Sign() => new(Math.Sign(X), Math.Sign(Z));
    public WDir OrthoL() => new(Z, -X);
    public WDir OrthoR() => new(-Z, X);
    public WDir MirrorX() => new(-X, Z);
    public WDir MirrorZ() => new(X, -Z);

    public float Dot(WDir a) => X * a.X + Z * a.Z;
    public float Cross(WDir b) => X * b.Z - Z * b.X;

    public WDir Rotate(WDir dir) => new(X * dir.Z + Z * dir.X, Z * dir.Z - X * dir.X);
    public WDir Rotate(Angle dir) => Rotate(dir.ToDirection());

    public float LengthSq() => X * X + Z * Z;
    public float Length() => MathF.Sqrt(LengthSq());

    public WDir Normalized()
    {
        float len = MathF.Sqrt(X * X + Z * Z);
        return len > 0f ? this / len : default;
    }

    public bool AlmostEqual(WDir b, float eps) => Math.Abs(X - b.X) <= eps && Math.Abs(Z - b.Z) <= eps;

    public WDir Scaled(float multiplier) => new(X * multiplier, Z * multiplier);
    public WDir Rounded() => new(MathF.Round(X), MathF.Round(Z));
    public WDir Rounded(float precision) => Scaled(1f / precision).Rounded().Scaled(precision);
    public WDir Floor() => new(MathF.Floor(X), MathF.Floor(Z));

    public Angle ToAngle() => new(MathF.Atan2(X, Z));

    public override string ToString() => $"({X:f3}, {Z:f3})";

    public bool Equals(WDir other) => this == other;
    public override bool Equals(object? obj) => obj is WDir other && Equals(other);
    public override int GetHashCode() => (X, Z).GetHashCode();

    public bool InRect(WDir direction, float lenFront, float lenBack, float halfWidth)
    {
        float along = Dot(direction);
        float perp = Dot(direction.OrthoL());
        return along >= -lenBack && along <= lenFront && Math.Abs(perp) <= halfWidth;
    }

    public bool InCross(WDir direction, float length, float halfWidth)
    {
        float along = Dot(direction);
        float perp = Math.Abs(Dot(direction.OrthoL()));
        bool inLongArm = along >= -length && along <= length && perp <= halfWidth;
        bool inShortArm = along >= -halfWidth && along <= halfWidth && perp <= length;
        return inLongArm || inShortArm;
    }
}
