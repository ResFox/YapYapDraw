using System.Numerics;
using YapYapDraw.Engine.Util;

namespace YapYapDraw.Engine.Struct;

public struct ActorCastInfo
{
    public ushort ActionId;

    public byte DisplayDelay;

    public float CastTime;

    public uint SourceId;

    public uint TargetId;

    public Angle Facing;

    public byte CanInterrupt;

    public Vector3 Pos;

    public Vector3 TargetPos;
}
