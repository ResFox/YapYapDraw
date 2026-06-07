using System;
using System.Globalization;

namespace YapYapDraw.Engine.Util;

public readonly struct Angle(float rad)
{
    public readonly float Rad = rad;

    public const float RadToDeg = 180f / MathF.PI;
    public const float DegToRad = MathF.PI / 180f;
    public const float HalfPi = MathF.PI / 2f;
    public const float DoublePI = MathF.PI * 2f;

    public static readonly Angle[] AnglesIntercardinals =
    [
        (-45.003f).Degrees(),
        44.998f.Degrees(),
        134.999f.Degrees(),
        (-135.005f).Degrees(),
    ];

    public static readonly Angle[] AnglesCardinals =
    [
        (-90.004f).Degrees(),
        (-0.003f).Degrees(),
        180f.Degrees(),
        89.999f.Degrees(),
    ];

    public float Deg => Rad * RadToDeg;

    public static Angle FromDirection(WDir dir) => new(MathF.Atan2(dir.X, dir.Z));

    public WDir ToDirection()
    {
        var (sin, cos) = Math.SinCos(Rad);
        return new WDir((float)sin, (float)cos);
    }

    public static bool operator ==(Angle left, Angle right) => left.Rad == right.Rad;
    public static bool operator !=(Angle left, Angle right) => left.Rad != right.Rad;
    public static Angle operator +(Angle a, Angle b) => new(a.Rad + b.Rad);
    public static Angle operator -(Angle a, Angle b) => new(a.Rad - b.Rad);
    public static Angle operator -(Angle a) => new(-a.Rad);
    public static Angle operator *(Angle a, float b) => new(a.Rad * b);
    public static Angle operator *(float a, Angle b) => new(a * b.Rad);
    public static Angle operator /(Angle a, float b) => new(a.Rad / b);
    public static bool operator >(Angle a, Angle b) => a.Rad > b.Rad;
    public static bool operator <(Angle a, Angle b) => a.Rad < b.Rad;
    public static bool operator >=(Angle a, Angle b) => a.Rad >= b.Rad;
    public static bool operator <=(Angle a, Angle b) => a.Rad <= b.Rad;

    public Angle Abs() => new(Math.Abs(Rad));
    public float Sin() => (float)Math.Sin(Rad);
    public float Cos() => (float)Math.Cos(Rad);
    public float Tan() => (float)Math.Tan(Rad);

    public static Angle Atan2(float y, float x) => new(MathF.Atan2(y, x));
    public static Angle Asin(float x) => new((float)Math.Asin(x));
    public static Angle Acos(float x) => new((float)Math.Acos(x));

    public Angle Round(float roundToNearestDeg) =>
        (MathF.Round(Deg / roundToNearestDeg) * roundToNearestDeg).Degrees();

    public Angle Normalized()
    {
        float r = Rad;
        while (r < -MathF.PI)
            r += MathF.PI * 2f;
        while (r > MathF.PI)
            r -= MathF.PI * 2f;
        return new Angle(r);
    }

    public bool AlmostEqual(Angle other, float epsRad) =>
        Math.Abs((this - other).Normalized().Rad) <= epsRad;

    public Angle DistanceToAngle(Angle other) => (other - this).Normalized();

    public Angle DistanceToRange(Angle min, Angle max)
    {
        Angle half = (max - min) * 0.5f;
        Angle dist = DistanceToAngle((min + max) * 0.5f);
        if (dist.Rad > half.Rad)
            return dist - half;
        if (dist.Rad < -half.Rad)
            return dist + half;
        return default;
    }

    public Angle ClosestInRange(Angle min, Angle max)
    {
        Angle half = (max - min) * 0.5f;
        Angle dist = DistanceToAngle((min + max) * 0.5f);
        if (dist.Rad > half.Rad)
            return min;
        if (dist.Rad < -half.Rad)
            return max;
        return this;
    }

    public override string ToString() => Deg.ToString("f3", CultureInfo.InvariantCulture);

    public bool Equals(Angle other) => Rad == other.Rad;
    public override bool Equals(object? obj) => obj is Angle other && Equals(other);
    public override int GetHashCode() => Rad.GetHashCode();
}
