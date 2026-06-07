using System;
using System.Collections.Generic;
using System.Numerics;
using Dalamud.Game;
using Dalamud.Game.ClientState.Objects.Types;
using YapYapDraw.Engine.Interop;
using Lumina.Excel.Sheets;
using Lumina.Text.ReadOnly;
using YapYapDraw.Engine.Enum;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.Struct;
using YapYapDraw.Engine.Util;
using YapYapDraw.Engine.Vfx;
using Action = Lumina.Excel.Sheets.Action;


namespace YapYapDraw.Engine.Element;

public static class SimpleElement
{
    public static void Circle(ActorCastInfo info)
    {
        Action row = Svc.Data.GetExcelSheet<Action>((ClientLanguage?)null, (string)null).GetRow((uint)info.ActionId);
        Circle(info.Pos, (int)((Action)(row)).EffectRange, 3000f, 0f, new HitCounter
        {
            ActionID = new HashSet<uint> { info.ActionId }
        });
    }

    public static StaticVfx Circle(ActorCastInfo info, float Radius, float Delay = 0f, Vector4? Color = null)
    {
        return Circle(info.Pos, Radius, info.CastTime * 1000f, Delay, new HitCounter
        {
            ActionID = new HashSet<uint> { info.ActionId }
        }, Color);
    }

    public static StaticVfx Circle(ulong ObjectId, float Radius, float CastTime = 3000f, float Delay = 0f, uint ActionId = 0u, Vector4? Color = null)
    {
        return Circle(ObjectId.GameObject(), Radius, CastTime, Delay, (ActionId != 0) ? new HitCounter
        {
            ActionID = new HashSet<uint> { ActionId }
        } : null, null, Color);
    }

    public static StaticVfx Circle(IGameObject? Object, float Radius, float CastTime = 3000f, float Delay = 0f, HitCounter? HitCounter = null, CountCheck? CountCheck = null, Vector4? Color = null)
    {
        DrawElement drawElement = new DrawElement
        {
            drawAvfx = ((!Color.HasValue) ? "general_1bxf" : "customCircle"),
            radiusX = Radius,
            radiusZ = Radius,
            destroyTime = CastTime,
            delayDrawTime = Delay,
            drawOnObject = true,
            hitCounter = HitCounter,
            CountCheck = CountCheck
        };
        if (Color.HasValue)
        {
            drawElement.refColor = Color.Value;
            drawElement.refTargetColor = Color.Value;
        }
        return DrawManager.Draw(drawElement, Object);
    }

    public static StaticVfx Circle(Vector3 Pos, float Radius, float CastTime = 3000f, float Delay = 0f, HitCounter? HitCounter = null, Vector4? Color = null)
    {
        DrawElement drawElement = new DrawElement
        {
            drawAvfx = ((!Color.HasValue) ? "general_1bxf" : "customCircle"),
            Position = Pos,
            drawOnObject = false,
            radiusX = Radius,
            radiusZ = Radius,
            destroyTime = CastTime,
            delayDrawTime = Delay,
            hitCounter = HitCounter
        };
        if (Color.HasValue)
        {
            drawElement.refColor = Color.Value;
            drawElement.refTargetColor = Color.Value;
        }
        return DrawManager.Draw(drawElement);
    }

    public static StaticVfx Rectangle(ActorCastInfo info)
    {
        Action row = Svc.Data.GetExcelSheet<Action>((ClientLanguage?)null, (string)null).GetRow((uint)info.ActionId);
        return DrawManager.Draw(new DrawElement
        {
            drawAvfx = "general02xf",
            Position = info.Pos,
            drawOnObject = false,
            radiusX = (float)(int)((Action)(row)).XAxisModifier / 2f,
            radiusZ = (int)((Action)(row)).EffectRange,
            refRotation = info.Facing,
            fixRotation = true,
            hitCounter = new HitCounter
            {
                ActionID = new HashSet<uint> { info.ActionId }
            }
        });
    }

    public static void Rectangle(ActorCastInfo info, float LengthFront, float HalfWidth, float LengthBack = 0f, Vector2? Offset = null, float Delay = 0f, Vector4? Color = null)
    {
        Rectangle(info.SourceId, LengthFront, HalfWidth, LengthBack, Offset, info.Facing, 3000f, Delay, info.ActionId, Color);
    }

    public static StaticVfx Rectangle(uint SourceId, float LengthFront, float HalfWidth, float LengthBack = 0f, Vector2? Offset = null, Angle Rotation = default, float CastTime = 3000f, float Delay = 0f, uint ActionId = 0u, Vector4? Color = null)
    {
        IGameObject source = SourceId.GameObject();
        if (source == null)
        {
            return null;
        }
        return Rectangle(source, LengthFront, HalfWidth, LengthBack, Offset, Rotation, CastTime, Delay, (ActionId != 0) ? new HitCounter
        {
            ActionID = new HashSet<uint> { ActionId }
        } : null, Color);
    }

    public static StaticVfx Rectangle(IGameObject Object, float LengthFront, float HalfWidth, float LengthBack = 0f, Vector2? Offset = null, Angle Rotation = default, float CastTime = 3000f, float Delay = 0f, HitCounter? HitCounter = null, Vector4? Color = null)
    {
        DrawElement drawElement = new DrawElement
        {
            drawAvfx = ((!Color.HasValue) ? "general02xf" : "customRect"),
            radiusX = HalfWidth,
            radiusZ = ((LengthBack == 0f) ? LengthFront : (LengthFront + LengthBack)),
            refOffsetX = ((!Offset.HasValue) ? 0f : Offset.Value.X),
            refOffsetZ = LengthBack + ((!Offset.HasValue) ? 0f : Offset.Value.Y),
            refRotation = Rotation,
            fixRotation = true,
            destroyTime = CastTime,
            delayDrawTime = Delay,
            hitCounter = HitCounter
        };
        if (Color.HasValue)
        {
            drawElement.refColor = Color.Value;
            drawElement.refTargetColor = Color.Value;
        }
        return DrawManager.Draw(drawElement, Object);
    }

    public static StaticVfx Rectangle(Vector3 Pos, float LengthFront, float HalfWidth, float LengthBack = 0f, Angle Rotation = default, float CastTime = 3000f, float Delay = 0f, HitCounter? HitCounter = null)
    {
        return DrawManager.Draw(new DrawElement
        {
            drawAvfx = "general02xf",
            Position = Pos,
            drawOnObject = false,
            radiusX = HalfWidth,
            radiusZ = ((LengthBack == 0f) ? LengthFront : (LengthFront + LengthBack)),
            refOffsetZ = LengthBack,
            refRotation = Rotation,
            fixRotation = true,
            destroyTime = CastTime,
            delayDrawTime = Delay,
            hitCounter = HitCounter
        });
    }

    public static StaticVfx RectangleMdl(IGameObject GameObject, float LengthFront, float HalfWidth, float LengthBack = 0f, Angle Rotation = default, float CastTime = 3000f, float Delay = 0f, HitCounter? hitCounter = null)
    {
        return DrawManager.Draw(new DrawElement
        {
            drawAvfx = "mdl_general03_o0e1",
            drawOnObject = true,
            radiusX = HalfWidth,
            radiusZ = ((LengthBack == 0f) ? LengthFront : (LengthFront + LengthBack)),
            refOffsetZ = LengthBack,
            refRotation = Rotation,
            fixRotation = true,
            destroyTime = CastTime,
            delayDrawTime = Delay,
            hitCounter = hitCounter
        }, GameObject);
    }

    public static StaticVfx RectangleMdl(ActorCastInfo info)
    {
        Action row = Svc.Data.GetExcelSheet<Action>((ClientLanguage?)null, (string)null).GetRow((uint)info.ActionId);
        return DrawManager.Draw(new DrawElement
        {
            drawAvfx = "mdl_general03_o0e1",
            Position = info.Pos,
            drawOnObject = false,
            radiusX = (float)(int)((Action)(row)).XAxisModifier / 2f,
            radiusZ = (int)((Action)(row)).EffectRange,
            refRotation = info.Facing,
            fixRotation = true,
            hitCounter = new HitCounter
            {
                ActionID = new HashSet<uint> { info.ActionId }
            }
        });
    }

    public static void RectangleToTarget(ActorCastInfo info, float Length, float HalfWidth)
    {
        RectangleToTarget(info.SourceId, info.TargetId, Length, HalfWidth, 3000f, info.ActionId);
    }

    public static void RectangleToTarget(uint sourceId, ulong targetId, float Length, float HalfWidth, float CastTime = 3000f, uint ActionId = 0u)
    {
        IGameObject source = sourceId.GameObject();
        if (source != null)
        {
            IGameObject target = targetId.GameObject();
            if (target != null)
            {
                RectangleToTarget(source, target, Length, HalfWidth, CastTime, (ActionId != 0) ? new HitCounter
                {
                    ActionID = new HashSet<uint> { ActionId }
                } : null);
            }
        }
    }

    public static void RectangleToTarget(IGameObject source, IGameObject target, float Length, float HalfWidth, float CastTime = 3000f, HitCounter? HitCounter = null)
    {
        DrawManager.Draw(new DrawElement
        {
            drawAvfx = "general02xf",
            radiusX = HalfWidth,
            radiusZ = Length,
            drawOnObject = true,
            target = target,
            destroyTime = CastTime,
            hitCounter = HitCounter
        }, source);
    }

    public static void RectangleToTarget(Vector3 origin, Vector3 targetPos, float Length, float HalfWidth, float CastTime = 3000f, HitCounter? HitCounter = null)
    {
        DrawManager.Draw(new DrawElement
        {
            drawAvfx = "general02xf",
            Position = origin,
            drawOnObject = false,
            radiusX = HalfWidth,
            radiusZ = Length,
            targetPosition = targetPos,
            destroyTime = CastTime,
            hitCounter = HitCounter
        });
    }

    public static void RectangleToPos(ActorCastInfo info)
    {
        IGameObject source = info.SourceId.GameObject();
        Vector3 delta = info.TargetPos - source.Position;
        Angle refRotation = MathF.Atan2(delta.X, delta.Z).Radians();
        Action row = Svc.Data.GetExcelSheet<Action>((ClientLanguage?)null, (string)null).GetRow((uint)info.ActionId);
        DrawManager.Draw(new DrawElement
        {
            drawAvfx = "general02xf",
            Position = source.Position,
            drawOnObject = false,
            radiusX = (float)(int)((Action)(row)).XAxisModifier / 2f,
            refRotation = refRotation,
            fixRotation = true,
            targetPosition = info.TargetPos,
            endToTarget = true,
            destroyTime = info.CastTime * 1000f
        });
    }

    public static List<StaticVfx> Cross(ActorCastInfo info)
    {
        Action row = Svc.Data.GetExcelSheet<Action>((ClientLanguage?)null, (string)null).GetRow((uint)info.ActionId);
        return Cross(info.Pos, (int)((Action)(row)).EffectRange, ((Action)(row)).XAxisModifier / 2, info.Facing, 3000f, 0f, new HitCounter
        {
            ActionID = new HashSet<uint> { info.ActionId }
        });
    }

    public static void Cross(ActorCastInfo info, float Length, float HalfWidth, float Delay = 0f)
    {
        Cross(info.SourceId, Length, HalfWidth, info.Facing, 3000f, Delay, info.ActionId);
    }

    public static List<StaticVfx> Cross(ulong ObjectId, float Length, float HalfWidth, Angle Rotation = default, float CastTime = 3000f, float Delay = 0f, uint ActionId = 0u)
    {
        IGameObject source = ObjectId.GameObject();
        if (source == null)
        {
            return new List<StaticVfx>();
        }
        return Cross(source, Length, HalfWidth, Rotation, CastTime, Delay, (ActionId != 0) ? new HitCounter
        {
            ActionID = new HashSet<uint> { ActionId }
        } : null);
    }

    public static List<StaticVfx> Cross(IGameObject Object, float Length, float HalfWidth, Angle Rotation = default, float CastTime = 3000f, float Delay = 0f, HitCounter? HitCounter = null)
    {
        DrawElement drawElement = new DrawElement
        {
            drawAvfx = "general_x02f",
            radiusX = HalfWidth,
            radiusZ = Length,
            drawOnObject = true,
            refRotation = Rotation,
            fixRotation = true,
            destroyTime = CastTime,
            delayDrawTime = Delay,
            hitCounter = HitCounter
        };
        List<StaticVfx> obj = new List<StaticVfx> { DrawManager.Draw(drawElement, Object) };
        drawElement.refRotation += 90.Degrees();
        obj.Add(DrawManager.Draw(drawElement, Object));
        return obj;
    }

    public static List<StaticVfx> Cross(Vector3 Pos, float Length, float HalfWidth, Angle Rotation, float CastTime = 3000f, float Delay = 0f, HitCounter? HitCounter = null, Vector4? Color = null)
    {
        DrawElement drawElement = new DrawElement
        {
            drawAvfx = ((!Color.HasValue) ? "general_x02f" : "customRect2"),
            Position = Pos,
            drawOnObject = false,
            radiusX = HalfWidth,
            radiusZ = Length,
            refRotation = Rotation,
            fixRotation = true,
            destroyTime = CastTime,
            delayDrawTime = Delay,
            hitCounter = HitCounter
        };
        if (Color.HasValue)
        {
            drawElement.refColor = Color.Value;
            drawElement.refTargetColor = Color.Value;
        }
        List<StaticVfx> obj = new List<StaticVfx> { DrawManager.Draw(drawElement) };
        drawElement.refRotation += 90.Degrees();
        obj.Add(DrawManager.Draw(drawElement));
        return obj;
    }

    public static StaticVfx Fan(ActorCastInfo info)
    {
        Action row = Svc.Data.GetExcelSheet<Action>((ClientLanguage?)null, (string)null).GetRow((uint)info.ActionId);
        DrawElement drawElement = new DrawElement();
        Omen value = ((Action)(row)).Omen.Value;
        ReadOnlySeString path = ((Omen)(value)).Path;
        drawElement.drawAvfx = ((ReadOnlySeString)(path)).ExtractText();
        drawElement.Position = info.Pos;
        drawElement.drawOnObject = false;
        drawElement.radiusX = (int)((Action)(row)).EffectRange;
        drawElement.radiusZ = (int)((Action)(row)).EffectRange;
        drawElement.refRotation = info.Facing;
        drawElement.fixRotation = true;
        drawElement.hitCounter = new HitCounter
        {
            ActionID = new HashSet<uint> { info.ActionId }
        };
        return DrawManager.Draw(drawElement);
    }

    public static void Fan(ActorCastInfo info, int Degree)
    {
        Action row = Svc.Data.GetExcelSheet<Action>((ClientLanguage?)null, (string)null).GetRow((uint)info.ActionId);
        byte effectRange = ((Action)(row)).EffectRange;
        Fan(info.Pos, (int)effectRange, Degree, info.Facing, 3000f, 0f, new HitCounter
        {
            ActionID = new HashSet<uint> { info.ActionId }
        });
    }

    public static StaticVfx Fan(ActorCastInfo info, float Length, int Degree, float Delay = 0f)
    {
        return Fan(info.SourceId, Length, Degree, info.Facing, 3000f, Delay, info.ActionId);
    }

    public static StaticVfx Fan(ulong sourceId, float Length, int Degree, Angle Rotation = default, float CastTime = 3000f, float Delay = 0f, uint ActionId = 0u)
    {
        IGameObject source = sourceId.GameObject();
        if (source == null)
        {
            return null;
        }
        return Fan(source, Length, Degree, Rotation, CastTime, Delay, (ActionId != 0) ? new HitCounter
        {
            ActionID = new HashSet<uint> { ActionId }
        } : null);
    }

    public static StaticVfx Fan(IGameObject? Object, float Length, int Degree, Angle Rotation = default, float CastTime = 3000f, float Delay = 0f, HitCounter? HitCounter = null, bool fixRotation = true)
    {
        DrawElement drawElement = new DrawElement
        {
            drawAvfx = ShapeUtil.GetGameFanOmen(Degree),
            refRadian = Degree.Degrees().Rad,
            radiusX = Length,
            radiusZ = Length,
            refRotation = Rotation,
            fixRotation = fixRotation,
            delayDrawTime = Delay,
            destroyTime = CastTime,
            hitCounter = HitCounter
        };
        if (drawElement.drawAvfx == "customFan")
        {
            drawElement.refColor = GroundOmen.enemyColor;
            drawElement.refTargetColor = GroundOmen.enemyColor;
        }
        return DrawManager.Draw(drawElement, Object);
    }

    public static StaticVfx Fan(Vector3 Pos, float Length, int Degree, Angle Rotation = default, float CastTime = 3000f, float Delay = 0f, HitCounter? HitCounter = null)
    {
        DrawElement drawElement = new DrawElement
        {
            drawAvfx = ShapeUtil.GetGameFanOmen(Degree),
            Position = Pos,
            drawOnObject = false,
            refRadian = Degree.Degrees().Rad,
            radiusX = Length,
            radiusZ = Length,
            refRotation = Rotation,
            fixRotation = true,
            delayDrawTime = Delay,
            destroyTime = CastTime,
            hitCounter = HitCounter
        };
        if (drawElement.drawAvfx == "customFan")
        {
            drawElement.refColor = GroundOmen.enemyColor;
            drawElement.refTargetColor = GroundOmen.enemyColor;
        }
        return DrawManager.Draw(drawElement);
    }

    public static void FanToTarget(ActorCastInfo info, float Length, int Degree, bool Follow = true)
    {
        FanToTarget(info.SourceId, info.TargetId, Length, Degree, Follow, info.Facing, 3000f, info.ActionId);
    }

    public static void FanToTarget(ulong sourceId, ulong targetId, float Length, int Degree, bool Follow = true, Angle Rotation = default, float CastTime = 3000f, uint ActionId = 0u)
    {
        IGameObject source = sourceId.GameObject();
        if (source != null)
        {
            IGameObject target = targetId.GameObject();
            if (target != null)
            {
                FanToTarget(source, target, Length, Degree, Follow, Rotation, 0f, CastTime, (ActionId != 0) ? new HitCounter
                {
                    ActionID = new HashSet<uint> { ActionId }
                } : null);
            }
        }
    }

    public static void FanToTarget(IGameObject source, IGameObject target, float Length, int Degree, bool Follow = true, Angle Rotation = default, float Delay = 0f, float CastTime = 3000f, HitCounter? HitCounter = null)
    {
        DrawElement drawElement = new DrawElement
        {
            drawAvfx = ShapeUtil.GetGameFanOmen(Degree),
            refRadian = Degree.Degrees().Rad,
            radiusX = Length,
            radiusZ = Length,
            destroyTime = CastTime,
            delayDrawTime = Delay,
            hitCounter = HitCounter
        };
        if (Follow)
        {
            drawElement.target = target;
        }
        else
        {
            drawElement.refRotation = Rotation;
            drawElement.fixRotation = true;
        }
        if (drawElement.drawAvfx == "customFan")
        {
            drawElement.refColor = GroundOmen.enemyColor;
            drawElement.refTargetColor = GroundOmen.enemyColor;
        }
        DrawManager.Draw(drawElement, source);
    }

    public static StaticVfx Donut(ActorCastInfo info)
    {
        Action row = Svc.Data.GetExcelSheet<Action>((ClientLanguage?)null, (string)null).GetRow((uint)info.ActionId);
        DrawElement drawElement = new DrawElement();
        Omen value = ((Action)(row)).Omen.Value;
        ReadOnlySeString path = ((Omen)(value)).Path;
        drawElement.drawAvfx = ((ReadOnlySeString)(path)).ExtractText();
        drawElement.Position = info.Pos;
        drawElement.drawOnObject = false;
        drawElement.radiusX = (int)((Action)(row)).EffectRange;
        drawElement.radiusZ = (int)((Action)(row)).EffectRange;
        drawElement.refRotation = info.Facing;
        drawElement.fixRotation = true;
        drawElement.destroyTime = info.CastTime * 1000f;
        drawElement.hitCounter = new HitCounter
        {
            ActionID = new HashSet<uint> { info.ActionId }
        };
        return DrawManager.Draw(drawElement);
    }

    public static StaticVfx Donut(ActorCastInfo info, float InnerRadius, float OuterRadius, float Delay = 0f, Vector4? Color = null)
    {
        return Donut(info.Pos, InnerRadius, OuterRadius, info.CastTime * 1000f, Delay, new HitCounter
        {
            ActionID = new HashSet<uint> { info.ActionId }
        }, Color);
    }

    public static StaticVfx Donut(ulong ObjectId, float InnerRadius, float OuterRadius, float CastTime = 3000f, float Delay = 0f, uint ActionId = 0u, Vector4? Color = null)
    {
        return Donut(ObjectId.GameObject(), InnerRadius, OuterRadius, CastTime, Delay, (ActionId != 0) ? new HitCounter
        {
            ActionID = new HashSet<uint> { ActionId }
        } : null, Color);
    }

    public static StaticVfx Donut(IGameObject? Object, float InnerRadius, float OuterRadius, float CastTime = 3000f, float Delay = 0f, HitCounter? HitCounter = null, Vector4? Color = null)
    {
        DrawElement drawElement = new DrawElement
        {
            drawAvfx = "customDonut",
            radiusX = OuterRadius,
            radiusZ = OuterRadius,
            refRadian = InnerRadius / OuterRadius,
            destroyTime = CastTime,
            delayDrawTime = Delay,
            hitCounter = HitCounter,
            refColor = GroundOmen.enemyColor,
            refTargetColor = GroundOmen.enemyColor
        };
        if (Color.HasValue)
        {
            drawElement.refColor = Color.Value;
            drawElement.refTargetColor = Color.Value;
        }
        return DrawManager.Draw(drawElement, Object);
    }

    public static StaticVfx Donut(Vector3 Pos, float InnerRadius, float OuterRadius, float CastTime = 3000f, float Delay = 0f, HitCounter? HitCounter = null, Vector4? Color = null)
    {
        return DrawManager.Draw(new DrawElement
        {
            drawAvfx = "customDonut",
            Position = Pos,
            drawOnObject = false,
            radiusX = OuterRadius,
            radiusZ = OuterRadius,
            refRadian = InnerRadius / OuterRadius,
            destroyTime = CastTime,
            delayDrawTime = Delay,
            refColor = ((!Color.HasValue) ? GroundOmen.enemyColor : Color.Value),
            refTargetColor = ((!Color.HasValue) ? GroundOmen.enemyColor : Color.Value),
            hitCounter = HitCounter
        });
    }

    public static void KnockBack(ActorCastInfo info, float Radius, float Delay = 0f)
    {
        KnockBack(info.SourceId, Radius, 3000f, Delay, info.ActionId);
    }

    public static void KnockBack(uint ObjectId, float Radius, float CastTime = 3000f, float Delay = 0f, uint ActionId = 0u)
    {
        IGameObject source = ObjectId.GameObject();
        if (source != null)
        {
            KnockBack(source, Radius, CastTime, Delay, (ActionId != 0) ? new HitCounter
            {
                ActionID = new HashSet<uint> { ActionId }
            } : null);
        }
    }

    public static void KnockBack(IGameObject Object, float Radius, float CastTime = 3000f, float Delay = 0f, HitCounter? HitCounter = null)
    {
        DrawManager.Draw(new DrawElement
        {
            drawAvfx = "m0501_nockback_omen01d1",
            radiusX = Radius,
            radiusZ = Radius,
            destroyTime = CastTime,
            delayDrawTime = Delay,
            drawOnObject = true,
            hitCounter = HitCounter
        }, Object);
    }

    public static void KnockBack(Vector3 Pos, float Radius, float CastTime = 3000f, float Delay = 0f, HitCounter? HitCounter = null)
    {
        DrawManager.Draw(new DrawElement
        {
            drawAvfx = "m0501_nockback_omen01d1",
            Position = Pos,
            drawOnObject = false,
            radiusX = Radius,
            radiusZ = Radius,
            destroyTime = CastTime,
            delayDrawTime = Delay,
            hitCounter = HitCounter
        });
    }

    public static void RectangleKnockBack(Vector3 Pos, float LengthFront, float HalfWidth, float LengthBack = 0f, Angle Rotation = default, float CastTime = 3000f, float Delay = 0f, HitCounter? HitCounter = null)
    {
        DrawManager.Draw(new DrawElement
        {
            drawAvfx = "m0792_knockaside_red02k1",
            Position = Pos,
            drawOnObject = false,
            radiusX = HalfWidth,
            radiusZ = ((LengthBack == 0f) ? LengthFront : (LengthFront + LengthBack)),
            refOffsetZ = LengthBack,
            refRotation = Rotation,
            fixRotation = true,
            destroyTime = CastTime,
            delayDrawTime = Delay,
            hitCounter = HitCounter
        });
    }

    public static void RectangleKnockBack2(Vector3 Pos, float LengthFront, float HalfWidth, float LengthBack = 0f, Angle Rotation = default, float CastTime = 3000f, float Delay = 0f, HitCounter? HitCounter = null)
    {
        DrawManager.Draw(new DrawElement
        {
            drawAvfx = "z6r1_b4_laser_01k1",
            Position = Pos,
            drawOnObject = false,
            radiusX = HalfWidth,
            radiusZ = ((LengthBack == 0f) ? LengthFront : (LengthFront + LengthBack)),
            refOffsetZ = LengthBack,
            refRotation = Rotation,
            fixRotation = true,
            destroyTime = CastTime,
            delayDrawTime = Delay,
            hitCounter = HitCounter
        });
    }

    public static void Triangle(ActorCastInfo info, float Radius, int Degree, float Delay = 0f)
    {
        Triangle(info.SourceId, Radius, Degree, info.Facing, 3000f, Delay, info.ActionId);
    }

    public static void Triangle(uint ObjectId, float Radius, int Degree, Angle Rotarion = default, float CastTime = 3000f, float Delay = 0f, uint ActionId = 0u)
    {
        IGameObject source = ObjectId.GameObject();
        if (source != null)
        {
            Triangle(source, Radius, Degree, Rotarion, CastTime, Delay, (ActionId != 0) ? new HitCounter
            {
                ActionID = new HashSet<uint> { ActionId }
            } : null);
        }
    }

    public static void Triangle(IGameObject Object, float Radius, int Degree, Angle Rotarion = default, float CastTime = 3000f, float Delay = 0f, HitCounter? HitCounter = null, CountCheck? CountCheck = null)
    {
        DrawManager.Draw(new DrawElement
        {
            drawAvfx = ShapeUtil.GetGameTriangleOmen(Degree),
            radiusX = Radius,
            radiusZ = Radius,
            refRotation = Rotarion,
            fixRotation = true,
            destroyTime = CastTime,
            delayDrawTime = Delay,
            drawOnObject = true,
            hitCounter = HitCounter,
            CountCheck = CountCheck
        }, Object);
    }

    public static void LineCircle(ActorCastInfo info, float StepRange, float TimeToMove, int Count, bool ShowAll = false)
    {
        Action row = Svc.Data.GetExcelSheet<Action>((ClientLanguage?)null, (string)null).GetRow((uint)info.ActionId);
        WPos wPos = new WPos(info.SourceId.GameObject().Position.X, info.SourceId.GameObject().Position.Z);
        WDir wDir = info.Facing.ToDirection();
        for (int i = 1; i <= Count; i++)
        {
            WPos wPos2 = wPos + StepRange * (float)i * wDir;
            DrawElement drawElement = new DrawElement
            {
                drawAvfx = "general_1bxf",
                Position = new Vector3(wPos2.X, 0f, wPos2.Z),
                drawOnObject = false,
                radiusX = (int)((Action)(row)).EffectRange,
                radiusZ = (int)((Action)(row)).EffectRange,
                delayDrawTime = ((i == 1) ? 0f : (info.CastTime * 1000f + (float)(i - 1) * TimeToMove)),
                destroyTime = ((i == 1) ? (info.CastTime * 1000f + TimeToMove) : TimeToMove)
            };
            if (ShowAll && i != 0)
            {
                drawElement.delayDrawTime = info.CastTime * 1000f;
                drawElement.destroyTime = (float)i * TimeToMove;
            }
            DrawManager.Draw(drawElement);
        }
    }

    public static void LineCircle(ActorCastInfo info, IGameObject GameObject, Angle Direction, float StepRange, float TimeToMove, int Count, bool ShowAll = false)
    {
        Action row = Svc.Data.GetExcelSheet<Action>((ClientLanguage?)null, (string)null).GetRow((uint)info.ActionId);
        WPos wPos = new WPos(GameObject.Position);
        WDir wDir = Direction.ToDirection();
        for (int i = 1; i <= Count; i++)
        {
            WPos wPos2 = wPos + StepRange * (float)i * wDir;
            DrawElement drawElement = new DrawElement
            {
                drawAvfx = "general_1bxf",
                Position = new Vector3(wPos2.X, 0f, wPos2.Z),
                drawOnObject = false,
                radiusX = (int)((Action)(row)).EffectRange,
                radiusZ = (int)((Action)(row)).EffectRange,
                delayDrawTime = ((i == 1) ? 0f : (info.CastTime * 1000f + (float)(i - 1) * TimeToMove)),
                destroyTime = ((i == 1) ? (info.CastTime * 1000f + TimeToMove) : TimeToMove)
            };
            if (ShowAll && i != 0)
            {
                drawElement.delayDrawTime = info.CastTime * 1000f;
                drawElement.destroyTime = (float)i * TimeToMove;
            }
            DrawManager.Draw(drawElement);
        }
    }

    public static void LineCircle(int EffectRange, float CastTime, IGameObject GameObject, Angle Direction, float StepRange, float TimeToMove, int Count, bool ShowAll = false)
    {
        WPos wPos = new WPos(GameObject.Position);
        WDir wDir = Direction.ToDirection();
        for (int i = 1; i <= Count; i++)
        {
            WPos wPos2 = wPos + StepRange * (float)i * wDir;
            DrawElement drawElement = new DrawElement
            {
                drawAvfx = "general_1bxf",
                Position = new Vector3(wPos2.X, 0f, wPos2.Z),
                drawOnObject = false,
                radiusX = EffectRange,
                radiusZ = EffectRange,
                delayDrawTime = ((i == 1) ? 0f : (CastTime * 1000f + (float)(i - 1) * TimeToMove)),
                destroyTime = ((i == 1) ? (CastTime * 1000f + TimeToMove) : TimeToMove)
            };
            if (ShowAll && i != 0)
            {
                drawElement.delayDrawTime = CastTime * 1000f;
                drawElement.destroyTime = (float)i * TimeToMove;
            }
            DrawManager.Draw(drawElement);
        }
    }

    public static void LineCircle(int EffectRange, float CastTime, IGameObject GameObject, Angle Direction, float StepRange, float TimeToMove, int Count, bool ShowAll = false, bool NoCast = false)
    {
        WPos wPos = new WPos(GameObject.Position);
        WDir wDir = Direction.ToDirection();
        for (int i = 1; i <= Count; i++)
        {
            WPos wPos2 = wPos + StepRange * (float)i * wDir;
            DrawElement drawElement = new DrawElement
            {
                drawAvfx = "general_1bxf",
                Position = new Vector3(wPos2.X, 0f, wPos2.Z),
                drawOnObject = false,
                radiusX = EffectRange,
                radiusZ = EffectRange,
                delayDrawTime = ((i == 1) ? 0f : (CastTime * 1000f + (float)(i - 1) * TimeToMove)),
                destroyTime = ((i == 1) ? (CastTime * 1000f + TimeToMove) : TimeToMove)
            };
            if (ShowAll && i != 0)
            {
                drawElement.delayDrawTime = CastTime * 1000f;
                if (NoCast)
                {
                    drawElement.delayDrawTime = 0f;
                }
                drawElement.destroyTime = (float)i * TimeToMove + CastTime * 1000f;
            }
            DrawManager.Draw(drawElement);
        }
    }

    public static void LineRect(ActorCastInfo info, float StepRange, float TimeToMove, int Count, bool ShowAll = false, Vector4 Color = default)
    {
        Action row = Svc.Data.GetExcelSheet<Action>((ClientLanguage?)null, (string)null).GetRow((uint)info.ActionId);
        WPos wPos = new WPos(info.SourceId.GameObject().Position);
        WDir wDir = info.Facing.ToDirection();
        for (int i = 0; i < Count; i++)
        {
            WPos wPos2 = wPos + StepRange * (float)i * wDir;
            DrawElement drawElement = new DrawElement
            {
                drawAvfx = ((Color == default) ? "general02xf" : "customRect"),
                Position = new Vector3(wPos2.X, 0f, wPos2.Z),
                drawOnObject = false,
                radiusX = ((Action)(row)).XAxisModifier / 2,
                radiusZ = (int)((Action)(row)).EffectRange,
                refRotation = info.SourceId.GameObject().Rotation.Radians(),
                fixRotation = true,
                delayDrawTime = ((i == 0) ? 0f : (info.CastTime * 1000f + (float)(i - 1) * TimeToMove)),
                destroyTime = ((i == 0) ? (info.CastTime * 1000f) : TimeToMove)
            };
            if (ShowAll && i != 0)
            {
                drawElement.delayDrawTime = info.CastTime * 1000f;
                drawElement.destroyTime = (float)i * TimeToMove;
            }
            if (Color != default)
            {
                drawElement.refColor = Color;
                drawElement.refTargetColor = Color;
            }
            DrawManager.Draw(drawElement);
        }
    }

    public static void LineRectOffset(ActorCastInfo info, float StepRange, float TimeToMove, int Count, float StartOffset = 0f, bool ShowAll = false, Vector4 Color = default)
    {
        Action row = Svc.Data.GetExcelSheet<Action>((ClientLanguage?)null, (string)null).GetRow((uint)info.ActionId);
        IGameObject source = info.SourceId.GameObject();
        WDir wDir = info.Facing.ToDirection();
        WPos wPos = new WPos(source.Position) + wDir * StartOffset;
        for (int i = 0; i < Count; i++)
        {
            WPos wPos2 = wPos + StepRange * (float)i * wDir;
            DrawElement drawElement = new DrawElement
            {
                drawAvfx = ((Color == default) ? "general02xf" : "customRect"),
                Position = new Vector3(wPos2.X, 0f, wPos2.Z),
                drawOnObject = false,
                radiusX = ((Action)(row)).XAxisModifier / 2,
                radiusZ = (int)((Action)(row)).EffectRange,
                refRotation = source.Rotation.Radians(),
                fixRotation = true,
                delayDrawTime = ((i == 0) ? 0f : (info.CastTime * 1000f + (float)(i - 1) * TimeToMove)),
                destroyTime = ((i == 0) ? (info.CastTime * 1000f) : TimeToMove)
            };
            if (ShowAll && i != 0)
            {
                drawElement.delayDrawTime = info.CastTime * 1000f;
                drawElement.destroyTime = (float)i * TimeToMove;
            }
            if (Color != default)
            {
                drawElement.refColor = Color;
                drawElement.refTargetColor = Color;
            }
            DrawManager.Draw(drawElement);
        }
    }

    public static void HotWing(IGameObject? obj, float Length, float HalfWidth, float Offset, float CastTime = 3000f, float Delay = 0f, HitCounter? HitCounter = null)
    {
        DrawElement obj2 = new DrawElement
        {
            drawAvfx = "general_x02f",
            radiusX = HalfWidth,
            radiusZ = Length,
            drawOnObject = true,
            refOffsetX = Offset,
            refRotation = obj.Rotation.Radians(),
            fixRotation = true,
            destroyTime = CastTime,
            delayDrawTime = Delay,
            hitCounter = HitCounter
        };
        DrawManager.Draw(obj2, obj);
        obj2.refOffsetX = 0f - Offset;
        DrawManager.Draw(obj2, obj);
    }

    public static void HotWing(Vector3 Pos, float Length, float HalfWidth, float Offset, Angle Rotation, float CastTime = 3000f, float Delay = 0f, HitCounter? HitCounter = null)
    {
        DrawElement obj = new DrawElement
        {
            drawAvfx = "general_x02f",
            Position = Pos,
            drawOnObject = false,
            radiusX = HalfWidth,
            radiusZ = Length,
            refOffsetX = Offset,
            refRotation = Rotation,
            fixRotation = true,
            destroyTime = CastTime,
            delayDrawTime = Delay,
            hitCounter = HitCounter
        };
        DrawManager.Draw(obj);
        obj.refOffsetX = 0f - Offset;
        DrawManager.Draw(obj);
    }

    public unsafe static void ShowText(string text, TextGimmickHintStyle style = default, int time = 5)
    {
        ((FFXIVClientStructs.FFXIV.Client.UI.RaptureAtkModule*)FFXIVClientStructs.FFXIV.Client.UI.RaptureAtkModule.Instance())
            ->ShowTextGimmickHint(text, style, time * 10);
    }
}
