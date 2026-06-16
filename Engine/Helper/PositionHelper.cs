using System;
using System.Numerics;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Interface.Utility;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using FFXIVClientStructs.FFXIV.Client.Graphics.Kernel;
using FFXIVClientStructs.FFXIV.Client.Graphics.Scene;

namespace YapYapDraw.Engine.Helper;

public static class PositionHelper
{
    public static unsafe Vector3 RenderPosition(this IGameObject obj)
    {
        if (obj.Address == IntPtr.Zero) return obj.Position;
        var go = (GameObject*)obj.Address;
        var draw = go->DrawObject;
        if (draw != null)
            return draw->Position;
        return obj.Position;
    }

    public static unsafe bool StableWorldToScreen(Vector3 world, out Vector2 screen)
    {
        screen = default;

        var controlCam = FFXIVClientStructs.FFXIV.Client.Game.Control.CameraManager.Instance()->GetActiveCamera();
        var renderCam = controlCam != null ? controlCam->SceneCamera.RenderCamera : null;
        if (renderCam == null)
            return Plugin.GameGui.WorldToScreen(world, out screen);

        var device = Device.Instance();
        if (device == null)
            return Plugin.GameGui.WorldToScreen(world, out screen);

        var view = renderCam->ViewMatrix;
        view.M44 = 1f;
        var viewProj = view * renderCam->ProjectionMatrix;

        var pp = Vector4.Transform(new Vector4(world, 1f), viewProj);
        if (pp.W <= 0f || Math.Abs(pp.W) < float.Epsilon)
            return false;

        var iw = 1f / pp.W;
        var windowPos = ImGuiHelpers.MainViewport.Pos;
        screen = new Vector2(
            0.5f * device->Width * (1f + pp.X * iw),
            0.5f * device->Height * (1f - pp.Y * iw)) + windowPos;
        return true;
    }

    public static bool AlmostEqual(this Vector2 pos, Vector2 pos2, float eps)
        => (pos - pos2).AlmostZero(eps);

    public static bool AlmostEqual(this Vector3 pos, Vector3 pos2, float eps)
        => (pos - pos2).AlmostZero(eps);

    public static bool AlmostZero(this Vector2 a, float eps)
        => Math.Abs(a.X) <= eps && Math.Abs(a.Y) <= eps;

    public static bool AlmostZero(this Vector3 a, float eps)
        => Math.Abs(a.X) <= eps && Math.Abs(a.Z) <= eps;

    public static Vector2 RotationDegress(this Vector2 offset, float degrees, bool clockwise = false)
    {
        var rad = degrees * (MathF.PI / 180f);
        if (clockwise)
            return new Vector2(
                offset.X * MathF.Cos(rad) - offset.Y * MathF.Sin(rad),
                offset.X * MathF.Sin(rad) + offset.Y * MathF.Cos(rad));
        return new Vector2(
            offset.X * MathF.Cos(rad) + offset.Y * MathF.Sin(rad),
            -offset.X * MathF.Sin(rad) + offset.Y * MathF.Cos(rad));
    }

    public static Vector2 ToVector2(this Vector3 pos) => new(pos.X, pos.Z);

    public static bool IsPointInsideField(Vector3 center, Vector3 point, float rotation = 0f, float halfSide = 10f)
    {
        var cos = Math.Cos(-rotation);
        var sin = Math.Sin(-rotation);
        var x = (point.X - center.X) * cos - (point.Z - center.Z) * sin + center.X;
        var z = (point.X - center.X) * sin + (point.Z - center.Z) * cos + center.Z;
        return x >= center.X - halfSide && x <= center.X + halfSide
            && z >= center.Z - halfSide && z <= center.Z + halfSide;
    }
}
