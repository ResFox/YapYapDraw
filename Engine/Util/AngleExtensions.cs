using System;

namespace YapYapDraw.Engine.Util;

public static class AngleExtensions
{
    public static Angle Radians(this float radians) => new(radians);
    public static Angle Degrees(this float degrees) => new(degrees * (MathF.PI / 180f));
    public static Angle Degrees(this int degrees) => new(degrees * (MathF.PI / 180f));
}
