using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.ClientState.Statuses;
using YapYapDraw.Engine.Interop;
using YapYapDraw.Engine.Interop.Game;
using YapYapDraw.Engine.Vfx;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.Memory;
using YapYapDraw.Engine.Struct.Vfx;
using YapYapDraw.Engine.Util;


namespace YapYapDraw.Engine.Vfx;

public class StaticVfx : BaseVfx
{
    private nint _ownerAddress = IntPtr.Zero;

    private readonly long _createdTick;

    private long _elapsed;

    public long initTime;

    private long _remainingTime = long.MaxValue;

    private int _hitCount;

    private Vector4 _currentColor = Vector4.One;

    public float height;

    public bool Init;

    public Actor? Actor { get; set; }

    public bool Enable { get; set; } = true;

    public Vector3 Scale { get; set; } = Vector3.One;

    public Vector3 Position { get; set; }

    public Vector3 Offset { get; set; } = Vector3.Zero;

    public Vector4 Color { get; set; } = Vector4.One;

    public Vector4 TargetColor { get; set; } = Vector4.One;

    public Angle Rotation { get; set; }

    public Angle OffsetRotation { get; set; }

    public float Radian { get; set; } = 1f;

    public IGameObject? Owner { get; set; }

    public IGameObject? Target { get; set; }

    public Vector3 TargetPosition { get; set; } = new Vector3(float.MinValue, 0f, 0f);

    public long DrawTime { get; set; }

    public long DelayTime { get; set; }

    public bool FixRotation { get; set; }

    public bool EndToTarget { get; set; }

    public bool AlwaysFaceCurrentTarget { get; set; }

    public bool AlwaysDrawOnCurrentTarget { get; set; }

    public bool OnlyVisible { get; set; }

    public Vector3 LastPosition { get; private set; }

    public Func<Vector3>? PositionCustomAction { get; set; }

    public Func<Vector3>? TargetPositionCustomAction { get; set; }

    public Func<Angle>? RotationCustomAction { get; set; }

    public HitCounter? HitCounter { get; set; }

    public DistanceCheck? DistanceCheck { get; set; }

    public TetherCheck? TetherCheck { get; set; }

    public StatusCheck? StatusCheck { get; set; }

    public CountCheck? CountCheck { get; set; }

    public KnockBackCheck? KnockBackCheck { get; set; }

    public WatchCheck? WatchCheck { get; set; }

    public StaticVfx(string path, Vector3 scale, IGameObject owner, Vector4 color)
        : this(path, scale, owner.Position, color, owner.Rotation.Radians())
    {
        Owner = owner;
        _ownerAddress = owner.Address;
    }

    public StaticVfx(string path, Vector3 scale, Vector3 position, Vector4 color, Angle rotation)
        : base(path)
    {
        Scale = scale;
        Position = position;
        Color = color;
        Rotation = rotation;
        _createdTick = Environment.TickCount64;
        FrameworkUpdateManager.StaticVfxs.Add(this);
        ClientOmenHooks.drawOmenElementList.Add(this);
    }

    public new unsafe void Update()
    {
        if (!Init)
        {
            return;
        }
        try
        {
            _elapsed = Environment.TickCount64 - _createdTick;
            if (initTime == 0L && _elapsed > DelayTime)
            {
                Create();
            }
            else
            {
                if (initTime == 0L)
                {
                    return;
                }
                height = Scale.Y;
                _remainingTime = DrawTime - (Environment.TickCount64 - initTime);
                if (HitCounter == null)
                {
                    if (_remainingTime < 0)
                    {
                        Remove();
                        return;
                    }
                }
                else if (_hitCount >= HitCounter.TargetHitCount)
                {
                    Remove();
                    return;
                }
                if (Actor != null)
                {
                    IGameObject actorObj = Svc.Objects.FirstOrDefault((IGameObject o) => o.GameObjectId == Actor.GameObjectID);
                    if (actorObj == null)
                    {
                        Remove();
                        return;
                    }
                    if (actorObj.IsDead)
                    {
                        IBattleChara battleActor = (IBattleChara)((actorObj is IBattleChara) ? actorObj : null);
                        if (battleActor != null && !battleActor.IsCasting)
                        {
                            Remove();
                            return;
                        }
                    }
                }
                if (Owner != null)
                {
                    IGameObject ownerObj = Svc.Objects.FirstOrDefault((IGameObject o) => o.Address == (IntPtr)_ownerAddress);
                    if (ownerObj == null || ownerObj.IsDead)
                    {
                        Remove();
                        return;
                    }
                    if (AlwaysFaceCurrentTarget)
                    {
                        Target = ownerObj.TargetObject;
                    }
                    if (AlwaysDrawOnCurrentTarget)
                    {
                        Owner = ownerObj.TargetObject;
                    }
                }
                try
                {
                    ApplyPosition();
                    ApplyRotation();
                    ApplyEndToTargetScale();
                    ApplyColorFade();
                    WatchCheckFunc();
                    ApplyDistanceCheck();
                    ApplyTetherCheck();
                    ApplyStatusCheck();
                    ApplyCountCheck();
                    ApplyKnockBackCheck();
                    if (Owner != null && OnlyVisible)
                    {
                        IGameObject? ownerLookup = Svc.Objects.FirstOrDefault((IGameObject o) => o.Address == (IntPtr)_ownerAddress);
                        ICharacter ownerChar = (ICharacter)((ownerLookup is ICharacter) ? ownerLookup : null);
                        if (ownerChar != null && !ownerChar.IsCharacterVisible())
                        {
                            height = 0f;
                        }
                    }
                    return;
                }
                finally
                {
                    if (Vfx != null)
                    {
                        if (!Enable)
                        {
                            Vfx->Scale.Y = 0f;
                            Vfx->Color.W = 0f;
                        }
                        else
                        {
                            Vfx->Scale.Y = height;
                            Vfx->Color.W = ((height == 0f) ? 0f : Color.W);
                        }
                        base.Update();
                    }
                }
            }
        }
        catch (Exception e)
        {
            e.Log();
        }
    }

    private unsafe void Create()
    {
        Vfx = (VfxStruct*)ClientOmenHooks.createStaticVfx(Path, "Client.System.Scheduler.Instance.VfxObject");
        if (Vfx != null && Vfx != (VfxStruct*)IntPtr.Zero)
        {
            VfxHandle = Vfx->Apricot->OmenVFXHandle;
            ClientOmenHooks.runStaticVfx((nint)Vfx, 0f, uint.MaxValue);
            ClientOmenHooks.TrackedVfxHandles.Add(((nint)Vfx, VfxHandle));
            UpdatePosition(Position);
            UpdateRotation(new Vector3(0f, 0f, Rotation.Rad));
            UpdateScale(Scale);
            UpdateColor(Color);
            base.Update();
            initTime = Environment.TickCount64;
        }
    }

    public bool IsWatchCheck(Vector3 TargetPos)
    {
        Vector3 vector = TargetPos - Owner.Position;
        vector.Y = 0f;
        if (Math.Abs(vector.X) < float.Epsilon && Math.Abs(vector.Z) < float.Epsilon)
        {
            return true;
        }
        float x = (float)Math.Sin(Owner.Rotation);
        float z = (float)Math.Cos(Owner.Rotation);
        Vector3 facingDir = new Vector3(x, 0f, z);
        float dot = facingDir.X * vector.X + facingDir.Z * vector.Z;
        float distance = (float)Math.Sqrt(vector.X * vector.X + vector.Z * vector.Z);
        return dot >= distance * 0.70710677f;
    }

    public void WatchCheckFunc()
    {
        if (WatchCheck != null && Owner != null)
        {
            if (IsWatchCheck(WatchCheck.WatchCheckPostion))
            {
                Color = WatchCheck.WatchWarnColor;
            }
            else
            {
                Color = WatchCheck.WatchSafeColor;
            }
        }
    }

    private Vector3 EffectiveTargetPosition()
    {
        if (TargetPositionCustomAction != null)
            return TargetPositionCustomAction();
        if (TargetPosition.X != float.MinValue)
            return TargetPosition;
        return Target?.RenderPosition() ?? Vector3.Zero;
    }

    private void ApplyPosition()
    {
        if (Owner != null)
        {
            if (Target == null || Svc.Objects.SearchById(Target.GameObjectId) != null)
            {
                IGameObject ownerObj = Svc.Objects.FirstOrDefault((IGameObject o) => o.Address == (IntPtr)_ownerAddress);
                if (ownerObj == null) return;
                Vector3 ownerPos = ownerObj.RenderPosition();
                Angle angle = (FixRotation ? Rotation : (ownerObj.Rotation.Radians() + Rotation));
                if (Target != null)
                {
                    Vector3 vector = Target.RenderPosition() - ownerPos;
                    angle = MathF.Atan2(vector.X, vector.Z).Radians();
                }
                else if (TargetPosition.X != float.MinValue || TargetPositionCustomAction != null)
                {
                    Vector3 far = EffectiveTargetPosition();
                    Vector3 vector2 = far - ownerPos;
                    angle = MathF.Atan2(vector2.X, vector2.Z).Radians();
                }
                Vector3 vector3 = RotateOffset(Offset, angle);
                Vector3 position = ownerPos - vector3;
                LastPosition = position;
                UpdatePosition(position);
            }
        }
        else if (Target == null || Svc.Objects.SearchById(Target.GameObjectId) != null)
        {
            Angle angle2 = Rotation;
            if (Target != null)
            {
                Vector3 vector4 = Target.RenderPosition() - Position;
                angle2 = MathF.Atan2(vector4.X, vector4.Z).Radians();
            }
            else if (TargetPosition.X != float.MinValue || TargetPositionCustomAction != null)
            {
                Vector3 far = EffectiveTargetPosition();
                Vector3 vector5 = far - Position;
                angle2 = MathF.Atan2(vector5.X, vector5.Z).Radians();
            }
            if (PositionCustomAction != null)
            {
                Position = PositionCustomAction();
            }
            Vector3 vector6 = RotateOffset(Offset, angle2);
            Vector3 position2 = Position - vector6;
            LastPosition = position2;
            UpdatePosition(position2);
        }
    }

    private static Vector3 RotateOffset(Vector3 offset, Angle angle)
    {
        float sin = MathF.Sin(angle.Rad);
        float cos = MathF.Cos(angle.Rad);
        return new Vector3(offset.X * cos + offset.Z * sin, z: (0f - offset.X) * sin + offset.Z * cos, y: offset.Y);
    }

    private void ApplyRotation()
    {
        if (Owner != null)
        {
            IGameObject ownerObj = Svc.Objects.FirstOrDefault((IGameObject o) => o.Address == (IntPtr)_ownerAddress);
            if (ownerObj == null) return;
            Vector3 ownerPos = ownerObj.RenderPosition();
            Angle angle = (FixRotation ? Rotation : (ownerObj.Rotation.Radians() + Rotation));
            if (Target != null)
            {
                Vector3 vector = Target.RenderPosition() - ownerPos;
                angle = MathF.Atan2(vector.X, vector.Z).Radians();
            }
            else if (TargetPosition.X != float.MinValue || TargetPositionCustomAction != null)
            {
                Vector3 far = EffectiveTargetPosition();
                Vector3 vector2 = far - ownerPos;
                angle = MathF.Atan2(vector2.X, vector2.Z).Radians();
            }
            UpdateRotation(new Vector3(0f, 0f, (angle + OffsetRotation).Rad));
        }
        else
        {
            Angle angle2 = RotationCustomAction != null ? RotationCustomAction() : Rotation;
            if (RotationCustomAction == null && Target != null)
            {
                Vector3 vector3 = Target.RenderPosition() - Position;
                angle2 = MathF.Atan2(vector3.X, vector3.Z).Radians();
            }
            else if (RotationCustomAction == null && (TargetPosition.X != float.MinValue || TargetPositionCustomAction != null))
            {
                Vector3 far = EffectiveTargetPosition();
                Vector3 vector4 = far - Position;
                angle2 = MathF.Atan2(vector4.X, vector4.Z).Radians();
            }
            UpdateRotation(new Vector3(0f, 0f, (angle2 + OffsetRotation).Rad));
        }
    }

    private void ApplyEndToTargetScale()
    {
        if (EndToTarget && Target != null)
        {
            IGameObject? ownerObj = Owner != null
                ? Svc.Objects.FirstOrDefault((IGameObject o) => o.Address == (IntPtr)_ownerAddress)
                : null;
            if (ownerObj != null)
            {
                UpdateScale(new Vector3(Scale.X, Scale.Y, Vector3.Distance(ownerObj.RenderPosition(), Target.RenderPosition())));
            }
            else
            {
                UpdateScale(new Vector3(Scale.X, Scale.Y, Vector3.Distance(Position, Target.RenderPosition())));
            }
        }
        if ((TargetPosition.X != float.MinValue || TargetPositionCustomAction != null) && EndToTarget)
        {
            Vector3 far = EffectiveTargetPosition();
            IGameObject? ownerObj = Owner != null
                ? Svc.Objects.FirstOrDefault((IGameObject o) => o.Address == (IntPtr)_ownerAddress)
                : null;
            if (ownerObj != null)
            {
                UpdateScale(new Vector3(Scale.X, Scale.Y, Vector3.Distance(ownerObj.RenderPosition(), far)));
            }
            else
            {
                UpdateScale(new Vector3(Scale.X, Scale.Y, Vector3.Distance(Position, far)));
            }
        }
    }

    private void ApplyColorFade()
    {
        if (HitCounter == null && WatchCheck == null)
        {
            if (_remainingTime >= 0)
            {
                float amount = 1f - (float)_remainingTime / (float)DrawTime;
                UpdateColor(_currentColor = Vector4.Lerp(Color, TargetColor, amount));
            }
        }
        else
        {
            UpdateColor(Color);
        }
    }

    public void OnHitEvent(uint ActionID, IGameObject? hitGameObject)
    {
        if (HitCounter != null && HitCounter.ActionID.Contains(ActionID))
        {
            if (HitCounter.HitTarget != null && HitCounter.HitTarget == hitGameObject)
            {
                _hitCount++;
            }
            else if (HitCounter.HitTarget == null)
            {
                _hitCount++;
            }
        }
    }

    public unsafe override void Remove()
    {
        FrameworkUpdateManager.StaticVfxs.Remove(this);
        ClientOmenHooks.drawOmenElementList.Remove(this);
        if (Vfx != null && ClientOmenHooks.removeStaticVfx != null)
        {
            ClientOmenHooks.removeStaticVfx((nint)Vfx, 10f);
            Vfx = null;
            VfxHandle = IntPtr.Zero;
        }
    }

    private void ApplyDistanceCheck()
    {
        if (DistanceCheck == null)
        {
            return;
        }
        float h = Scale.Y;
        switch (DistanceCheck.CheckType)
        {
        case 0:
            if (DistanceCheck.CheckObject != null && Target != null && !DistanceCheck.CheckObject.SortedByRange().Take(DistanceCheck.Count).Contains(Target))
            {
                h = 0f;
            }
            break;
        case 1:
            if (DistanceCheck.CheckObject != null && Target != null && !DistanceCheck.CheckObject.SortedByRange().TakeLast(DistanceCheck.Count).Contains(Target))
            {
                h = 0f;
            }
            break;
        case 2:
            if (DistanceCheck.CheckObject != null && Owner != null && !DistanceCheck.CheckObject.SortedByRange().Take(DistanceCheck.Count).Contains(Owner))
            {
                h = 0f;
            }
            break;
        case 3:
            if (DistanceCheck.CheckObject != null && Owner != null && !DistanceCheck.CheckObject.SortedByRange().TakeLast(DistanceCheck.Count).Contains(Owner))
            {
                h = 0f;
            }
            break;
        case 4:
            if (Target != null && !DistanceCheck.Position.SortedByRange().Take(DistanceCheck.Count).Contains(Target))
            {
                h = 0f;
            }
            break;
        case 5:
            if (Target != null && !DistanceCheck.Position.SortedByRange().TakeLast(DistanceCheck.Count).Contains(Target))
            {
                h = 0f;
            }
            break;
        case 6:
            if (Owner != null && !DistanceCheck.Position.SortedByRange().Take(DistanceCheck.Count).Contains(Owner))
            {
                h = 0f;
            }
            break;
        case 7:
            if (Owner != null && !DistanceCheck.Position.SortedByRange().TakeLast(DistanceCheck.Count).Contains(Owner))
            {
                h = 0f;
            }
            break;
        case 8:
            if (DistanceCheck.CheckObject != null && !DistanceCheck.Position.SortedByRange().Take(DistanceCheck.Count).Contains(DistanceCheck.CheckObject))
            {
                h = 0f;
            }
            break;
        case 9:
            if (DistanceCheck.CheckObject != null && !DistanceCheck.Position.SortedByRange().TakeLast(DistanceCheck.Count).Contains(DistanceCheck.CheckObject))
            {
                h = 0f;
            }
            break;
        }
        height = h;
    }

    private void ApplyTetherCheck()
    {
        if (TetherCheck == null || Owner == null)
        {
            return;
        }
        float h = Scale.Y;
        if (TetherCheck.CheckType == 0)
        {
            if (Target != null)
            {
                if (Data.TetherPlayer.FirstOrDefault((YapYapDraw.Engine.Memory.TetherInfo t) => t.From == Target.GameObjectId && TetherCheck.TetherID.Contains(t.TetherID)) != null)
                {
                    YapYapDraw.Engine.Memory.TetherInfo tetherInfo = Data.TetherPlayer.FirstOrDefault((YapYapDraw.Engine.Memory.TetherInfo t) => t.From == Target.GameObjectId && TetherCheck.TetherID.Contains(t.TetherID));
                    if (Owner.GameObjectId != tetherInfo.To)
                    {
                        h = 0f;
                    }
                }
                else
                {
                    h = 0f;
                }
            }
            else if (Data.TetherPlayer.FirstOrDefault((YapYapDraw.Engine.Memory.TetherInfo t) => t.From == Owner.GameObjectId && TetherCheck.TetherID.Contains(t.TetherID)) == null)
            {
                h = 0f;
            }
        }
        if (TetherCheck.CheckType == 1)
        {
            if (Target != null)
            {
                if (Data.TetherPlayer.FirstOrDefault((YapYapDraw.Engine.Memory.TetherInfo t) => t.From == Owner.GameObjectId && TetherCheck.TetherID.Contains(t.TetherID)) != null)
                {
                    YapYapDraw.Engine.Memory.TetherInfo tetherInfo2 = Data.TetherPlayer.FirstOrDefault((YapYapDraw.Engine.Memory.TetherInfo t) => t.From == Owner.GameObjectId && TetherCheck.TetherID.Contains(t.TetherID));
                    if (Target.GameObjectId != tetherInfo2.To)
                    {
                        h = 0f;
                    }
                }
                else
                {
                    h = 0f;
                }
            }
            else if (Data.TetherPlayer.FirstOrDefault((YapYapDraw.Engine.Memory.TetherInfo t) => t.To == Owner.GameObjectId && TetherCheck.TetherID.Contains(t.TetherID)) == null)
            {
                h = 0f;
            }
        }
        height = h;
    }

    private void ApplyStatusCheck()
    {
        if (StatusCheck == null)
        {
            return;
        }
        IGameObject checkObject = StatusCheck.CheckObject;
        IBattleChara chara = (IBattleChara)((checkObject is IBattleChara) ? checkObject : null);
        if (chara == null)
        {
            return;
        }
        float h = Scale.Y;
        if (!StatusCheck.Reverse)
        {
            if (chara.StatusList.All((IStatus s) => s.StatusId != StatusCheck.Status))
            {
                h = 0f;
            }
        }
        else if (chara.StatusList.Any((IStatus s) => s.StatusId == StatusCheck.Status))
        {
            h = 0f;
        }
        height = h;
    }

    private void ApplyCountCheck()
    {
        if (CountCheck == null)
        {
            return;
        }
        float alpha = Scale.Y;
        int nearbyCount = 0;
        foreach (IGameObject allPlayer in PlayerHelper.AllPlayers)
        {
            if (allPlayer != CountCheck?.CheckObject && CountCheck?.CheckObject.DistanceToTarget(allPlayer) < Scale.X)
            {
                nearbyCount++;
            }
        }
        if (nearbyCount == CountCheck?.Count)
        {
            alpha *= CountCheck.SafeAlpha;
        }
        height = alpha;
    }

    private void ApplyKnockBackCheck()
    {
        if (KnockBackCheck == null || Owner == null)
        {
            return;
        }
        if (KnockBackCheck.OriginPos.HasValue)
        {
            Vector3 vector = Owner.Position - KnockBackCheck.OriginPos.Value;
            Angle angle = MathF.Atan2(vector.X, vector.Z).Radians();
            if (KnockBackCheck.Reverse)
            {
                angle += 180.Degrees();
            }
            UpdateRotation(new Vector3(0f, 0f, angle.Rad));
        }
        else if (KnockBackCheck.Angle.HasValue)
        {
            UpdateRotation(new Vector3(0f, 0f, KnockBackCheck.Angle.Value.Rad));
        }
        if (KnockBackCheck.Antiable && Owner.KnockBackAnti())
        {
            height = 0f;
        }
    }
}
