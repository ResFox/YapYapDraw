using System;
using System.Numerics;
using Dalamud.Game.ClientState.Objects.Types;
using YapYapDraw.Engine.Interop.ActionEffect;
using YapYapDraw.Engine.Util;

namespace YapYapDraw.Engine.Struct;

public struct ActorAbilityInfo
{
    public uint ActionId;

    public IGameObject Source;

    public IGameObject? Target;

    public TargetEffect[] TargetEffects;

    public Angle Rotation;

    public Vector3 Pos;
}
