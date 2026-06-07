using System.Numerics;

namespace YapYapDraw.Engine.Struct.Vfx;

public struct Quat
{
    public float X;

    public float Z;

    public float Y;

    public float W;

    public static implicit operator Vector4(Quat pos)
    {
        return new Vector4(pos.X, pos.Y, pos.Z, pos.W);
    }
}
