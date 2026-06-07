using System;
using Dalamud.Game;
using Dalamud.Game.ClientState.Objects.Types;
using YapYapDraw.Engine.Interop;
using Lumina.Excel.Sheets;
using Lumina.Text.ReadOnly;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Enum;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.Struct;
using YapYapDraw.Engine.Util;
using Action = Lumina.Excel.Sheets.Action;

namespace YapYapDraw.Engine.Module;

public static class AutoDrawModule
{
    public static void Run(ActorCastInfo info)
    {
        var source = info.SourceId.GameObject();
        if (source == null) return;

        var target = info.TargetId != 3758096384u && info.TargetId != info.SourceId
            ? info.TargetId.GameObject()
            : null;

        var row = Svc.Data.GetExcelSheet<Action>((ClientLanguage?)null, (string?)null).GetRow(info.ActionId);
        var shape = ShapeUtil.GetShape(row.CastType);
        if (row.CastType == 1) return;

        if (row.Omen.IsValid)
        {
            var omen = row.Omen.Value;
            if (!string.IsNullOrEmpty(omen.Path.ExtractText()) && info.DisplayDelay == 0)
                return;
        }

        if (shape == Shape.Circle && row.EffectRange >= 50)
            return;

        var delay = MathF.Max(info.DisplayDelay / 10f - 4f, 0f) * 1000f;
        var element = new DrawElement
        {
            Actor = IGameObjectHelper.Find(info.SourceId),
            drawType = ElementType.Omen,
            Position = info.Pos,
            drawOnObject = false,
            radiusX = row.EffectRange,
            radiusZ = row.EffectRange,
            target = target,
            refRotation = info.Facing,
            delayDrawTime = delay,
            destroyTime = info.CastTime * 1000f - delay,
        };

        if (!TrySetOmenPath(row, ref element))
        {
            element.drawAvfx = ShapeToAvfx(shape);
            if (string.IsNullOrEmpty(element.drawAvfx))
                return;
        }

        ApplyShapeSizing(ref element, shape, row, source);
        Draw(element, shape, source, info);
    }

    private static bool TrySetOmenPath(Action row, ref DrawElement element)
    {
        if (!row.Omen.IsValid) return false;
        var path = row.Omen.Value.Path.ExtractText();
        if (string.IsNullOrEmpty(path)) return false;
        element.drawAvfx = path;
        return true;
    }

    private static string ShapeToAvfx(Shape shape) => shape switch
    {
        Shape.Circle => "general_1bxf",
        Shape.Rectangle or Shape.RectToTarget => "general02xf",
        Shape.Cross => "general_x02f",
        Shape.Triangle => "x6d3_b2_triangle90_p1",
        _ => string.Empty,
    };

    private static void ApplyShapeSizing(ref DrawElement element, Shape shape, Action row, IGameObject source)
    {
        element.radiusX = shape is Shape.Rectangle or Shape.RectToTarget
            ? row.XAxisModifier / 2f
            : element.radiusX;

        switch (row.CastType)
        {
            case 3:
            case 5:
                element.radiusX += source.HitboxRadius;
                element.radiusZ += source.HitboxRadius;
                break;
            case 4:
                element.radiusZ += source.HitboxRadius;
                break;
        }

        if (shape == Shape.RectToTarget)
            element.endToTarget = true;
    }

    private static void Draw(DrawElement element, Shape shape, IGameObject source, ActorCastInfo info)
    {
        if (shape == Shape.RectToTarget)
        {
            element.Position = source.Position;
            if (info.TargetId == 3758096384u)
                element.targetPosition = info.TargetPos;
            DrawManager.Draw(element);
            return;
        }

        if (element.target != null && shape == Shape.Circle)
        {
            element.drawOnObject = true;
            DrawManager.Draw(element, element.target);
            return;
        }

        DrawManager.Draw(element);
        if (shape == Shape.Cross)
        {
            element.refRotation += 90.Degrees();
            DrawManager.Draw(element);
        }
    }
}
