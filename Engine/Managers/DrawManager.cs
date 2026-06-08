using System;
using System.Collections.Generic;
using System.Numerics;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Utility;
using YapYapDraw.Engine.Interop;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Enum;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.Memory;
using YapYapDraw.Engine.Properties;
using YapYapDraw.Engine.Vfx;

namespace YapYapDraw.Engine.Managers;

public class DrawManager
{
    private static readonly Dictionary<string, Action<DrawElement>> CustomVfxBuilders = new Dictionary<string, Action<DrawElement>>
    {
        ["customFan"] = e =>
        {
            OmenResourceCache.GetFan(e.refRadian, out string drawAvfx);
            e.drawAvfx = drawAvfx;
        },
        ["customCircle"] = e =>
        {
            OmenResourceCache.GetCircle(out string drawAvfx);
            e.drawAvfx = drawAvfx;
        },
        ["customDonut"] = e =>
        {
            OmenResourceCache.GetDonut(e.refRadian, out string drawAvfx);
            e.drawAvfx = drawAvfx;
        },
        ["customRect"] = e =>
        {
            OmenResourceCache.GetRect(out string drawAvfx);
            e.drawAvfx = drawAvfx;
        },
        ["customRect2"] = e =>
        {
            OmenResourceCache.GetRect2(out string drawAvfx);
            e.drawAvfx = drawAvfx;
        },
        ["tank_fan90"] = e =>
        {
            OmenResourceCache.RegisterRaw(Resources.TankFan90, "vfx/omen/eff/yd/tankfan90.avfx");
            e.drawAvfx = "vfx/omen/eff/yd/tankfan90.avfx";
            e.radiusX = 1f;
            e.radiusY = 1f;
            e.radiusZ = 1f;
        },
        ["share2_6m"] = e =>
        {
            OmenResourceCache.RegisterRaw(Resources.Share2_6m_5s_omen, "vfx/omen/eff/yd/share2_6m.avfx");
            e.drawAvfx = "vfx/omen/eff/yd/share2_6m.avfx";
            e.radiusX = 1f;
            e.radiusY = 2f;
            e.radiusZ = 1f;
        },
        ["eye_warn"] = e =>
        {
            OmenResourceCache.RegisterRaw(Resources.eyewarn, "vfx/omen/eff/yd/eyewarn.avfx");
            e.drawAvfx = "vfx/omen/eff/yd/eyewarn.avfx";
            e.radiusX = 1f;
            e.radiusY = 1f;
            e.radiusZ = 1f;
        },
        ["e5d1_b1_kblaser_t1"] = e =>
        {
            e.refColor = new Vector4(1f, 1f, 1f, 2.5f);
            e.refTargetColor = new Vector4(1f, 1f, 1f, 2.5f);
        },
        ["tower_noc"] = e =>
        {
            e.drawAvfx = SilentOmen("tower_noc", "vfx/omen/eff/yd/tower_noc.avfx", GroundOmen.SingleTower);
        },
        ["knockback_noc"] = e =>
        {
            e.drawAvfx = SilentOmen("knockback_noc", "vfx/omen/eff/yd/knockback_noc.avfx", GroundOmen.KnockBack);
        },
        ["laser_noc"] = e =>
        {
            e.drawAvfx = SilentOmen("laser_noc", "vfx/omen/eff/yd/laser_noc.avfx", GroundOmen.ArrowRect);
            e.refColor = new Vector4(1f, 1f, 1f, 2.5f);
            e.refTargetColor = new Vector4(1f, 1f, 1f, 2.5f);
        },
        ["tank_lockon_3m_5s_noc"] = e =>
        {
            OmenResourceCache.RegisterRaw(Resources.tank_lockon_3m_5s_noc, "vfx/omen/eff/yd/tank_lockon_3m_5s_noc.avfx");
            e.drawAvfx = "vfx/omen/eff/yd/tank_lockon_3m_5s_noc.avfx";
            e.radiusX = 1f;
            e.radiusY = 1f;
            e.radiusZ = 1f;
        },
        ["tank_lockon_5m_5s_noc"] = e =>
        {
            OmenResourceCache.RegisterRaw(Resources.tank_lockon_5m_5s_noc, "vfx/omen/eff/yd/tank_lockon_5m_5s_noc.avfx");
            e.drawAvfx = "vfx/omen/eff/yd/tank_lockon_5m_5s_noc.avfx";
            e.radiusX = 1f;
            e.radiusY = 1f;
            e.radiusZ = 1f;
        },
        ["ShareLazerGround5s"] = e =>
        {
            OmenResourceCache.RegisterRaw(Resources.ShareLazer5sGround, "vfx/omen/eff/yd/share_lazer_5s_ground.avfx");
            e.drawAvfx = "vfx/omen/eff/yd/share_lazer_5s_ground.avfx";
            e.radiusX = 1f;
            e.radiusY = 1f;
            e.radiusZ = 1f;
        }
    };

    // Registers a sound-stripped copy of an omen and returns its path. Falls back
    // to the stock omen (with its sound) when the silenced asset isn't bundled.
    private static string SilentOmen(string resourceName, string path, string fallback)
    {
        byte[]? data = Resources.TryGet(resourceName);
        if (data == null)
            return fallback;
        OmenResourceCache.RegisterRaw(data, path);
        return path;
    }

    public static List<StaticVfx> Draw(DrawElement element, List<IGameObject> target, IGameObject? castObject = null)
    {
        List<StaticVfx> list = new List<StaticVfx>();
        if (StringExtensions.IsNullOrEmpty(element.drawAvfx))
        {
            return list;
        }
        foreach (IGameObject item in target)
        {
            StaticVfx staticVfx = Draw(element, item, castObject);
            if (staticVfx != null)
            {
                list.Add(staticVfx);
            }
        }
        return list;
    }

    public static StaticVfx Draw(DrawElement element, IGameObject? target = null, IGameObject? castObject = null)
    {
        if (StringExtensions.IsNullOrEmpty(element.drawAvfx))
        {
            return null;
        }
        if (element.drawOnObject)
        {
            if (target == null)
            {
                return null;
            }
            if (Svc.Objects.SearchById(target.GameObjectId) == null)
            {
                return null;
            }
        }
        switch (element.drawType)
        {
        case ElementType.Omen:
            return DrawOmen(target, element);
        case ElementType.LockOn:
            DrawLockOn(target, castObject, element);
            return null;
        case ElementType.Channeling:
            DrawChanneling(target, castObject, element);
            return null;
        case ElementType.RawAvfx:
            DrawRawAvfx(target, castObject, element);
            return null;
        default:
            return null;
        }
    }

    private static StaticVfx DrawOmen(IGameObject target, DrawElement element)
    {
        if (CustomVfxBuilders.TryGetValue(element.drawAvfx, out Action<DrawElement> value))
        {
            value(element);
        }
        StaticVfx staticVfx = CreateStaticVfx(element, target);
        ApplyElement(staticVfx, element);
        staticVfx.Init = true;
        return staticVfx;
    }

    private static StaticVfx CreateStaticVfx(DrawElement element, IGameObject target)
    {
        string path = ((element.drawAvfx.Split('/').Length == 1) ? element.drawAvfx.Omen() : element.drawAvfx);
        Vector3 scale = new Vector3(element.radiusX, element.radiusY, element.radiusZ);
        if (!element.drawOnObject)
        {
            return new StaticVfx(path, scale, element.Position, element.refColor, element.refRotation);
        }
        return new StaticVfx(path, scale, target, element.refColor);
    }

    private static void ApplyElement(StaticVfx vfx, DrawElement element)
    {
        vfx.Actor = element.Actor;
        vfx.Enable = element.Enable;
        vfx.Offset = new Vector3(element.refOffsetX, element.refOffsetY, element.refOffsetZ);
        vfx.Color = element.refColor;
        vfx.TargetColor = element.refTargetColor;
        vfx.Rotation = element.refRotation;
        vfx.OffsetRotation = element.refOffsetRotation;
        vfx.Radian = element.refRadian;
        vfx.Target = element.target;
        vfx.TargetPosition = element.targetPosition;
        vfx.DrawTime = (long)element.destroyTime;
        vfx.DelayTime = (long)element.delayDrawTime;
        vfx.FixRotation = element.fixRotation;
        vfx.EndToTarget = element.endToTarget;
        vfx.AlwaysFaceCurrentTarget = element.alwaysFaceCurrentTarget;
        vfx.AlwaysDrawOnCurrentTarget = element.alwaysDrawOnCurrentTarget;
        vfx.OnlyVisible = element.OnlyVisible;
        vfx.PositionCustomAction = element.PositionCustomAction;
        vfx.HitCounter = element.hitCounter;
        vfx.DistanceCheck = element.distanceCheck;
        vfx.TetherCheck = element.TetherCheck;
        vfx.StatusCheck = element.StatusCheck;
        vfx.CountCheck = element.CountCheck;
        vfx.KnockBackCheck = element.KnockBackCheck;
        vfx.WatchCheck = element.WatchCheck;
    }

    private static void DrawLockOn(IGameObject target, IGameObject? castObject, DrawElement element)
    {
        if (castObject == null)
            castObject = target;
        new ActorVfx(element.drawAvfx.LockOn(), castObject.Address, target.Address).DelayTime = (long)element.delayDrawTime;
    }

    private static void DrawChanneling(IGameObject target, IGameObject? castObject, DrawElement element)
    {
        if (castObject == null)
            castObject = target;
        new ActorVfx(element.drawAvfx.Channeling(), castObject.Address, target.Address)
        {
            DelayTime = (long)element.delayDrawTime,
            DestroyAt = (long)element.destroyTime,
            HitCounter = element.hitCounter,
            StatusCheck = element.StatusCheck
        };
    }

    private static void DrawRawAvfx(IGameObject target, IGameObject? castObject, DrawElement element)
    {
        if (castObject == null)
            castObject = target;
        new ActorVfx(element.drawAvfx, castObject.Address, target.Address)
        {
            DelayTime = (long)element.delayDrawTime,
            DestroyAt = (long)element.destroyTime
        };
    }
}
