using System;

namespace YapYapDraw.Engine.Util;

public static class Intersect
{
    public static bool CircleCone(WDir circleOffset, float circleRadius, float coneRadius, WDir coneDir, Angle halfAngle)
    {
        var distSq = circleOffset.LengthSq();
        var rSq = circleRadius * circleRadius;
        if (distSq <= rSq) return true;

        var maxDist = circleRadius + coneRadius;
        if (distSq > maxDist * maxDist) return false;
        if (halfAngle.Rad >= MathF.PI) return true;

        var inFront = circleOffset.Dot(coneDir) > 0f;
        var ortho = coneDir.OrthoL();
        var sin = halfAngle.Sin();
        var orthoDot = circleOffset.Dot(ortho);
        var edge = halfAngle.Rad - MathF.PI / 2f;
        if (edge < 0f ? inFront && orthoDot * orthoDot <= distSq * sin * sin
            : edge > 0f ? inFront || orthoDot * orthoDot >= distSq * sin * sin
            : inFront)
            return true;

        if (orthoDot < 0f) ortho = -ortho;
        var edgeDir = coneDir * halfAngle.Cos() + ortho * sin;
        if (Math.Abs(circleOffset.Cross(edgeDir)) > circleRadius) return false;
        var along = circleOffset.Dot(edgeDir);
        if (along < 0f) return false;
        if (along <= coneRadius) return true;
        return (circleOffset - edgeDir * coneRadius).LengthSq() <= rSq;
    }

    public static bool CircleCone(WPos circleCenter, float circleRadius, WPos coneCenter, float coneRadius, WDir coneDir, Angle halfAngle)
        => CircleCone(circleCenter - coneCenter, circleRadius, coneRadius, coneDir, halfAngle);
}
