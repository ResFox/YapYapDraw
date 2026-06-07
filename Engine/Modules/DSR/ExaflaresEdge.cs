using System;
using System.Collections.Generic;
using System.Numerics;
using Dalamud.Game.ClientState.Objects.Types;
using YapYapDraw.Engine.Interop;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;
using YapYapDraw.Engine.Util;

namespace YapYapDraw.Modules.DSR;

public class ExaflaresEdge : ISpecialAction
{
    public override string Name => "Exaflare's Edge";

    public override uint Phase => 7u;

    public override HashSet<uint> ActionID => new HashSet<uint> { 28060u };

    public override void OnActionCast(ActorCastInfo info)
    {
        IGameObject source = Svc.Objects.SearchById((ulong)info.SourceId);
        if (source != null)
        {
            Vector3 position = source.Position;
            Angle facing = info.Facing;
            for (int i = 0; i < 5; i++)
            {
                Vector3 forwardPos = position - RotateVector(new Vector3(0f, 0f, -7 * (i + 1)), facing.Rad);
                DrawManager.Draw(new DrawElement
                {
                    drawAvfx = "general_1bxf",
                    Position = new Vector3(forwardPos.X, 0f, forwardPos.Z),
                    drawOnObject = false,
                    radiusX = 6f,
                    radiusZ = 6f,
                    delayDrawTime = 6900 + i * 1900,
                    destroyTime = 1900f
                }, (IGameObject?)Svc.Objects.LocalPlayer);
                Vector3 leftPos = position - RotateVector(new Vector3(-7 * (i + 1), 0f, 0f), facing.Rad);
                DrawManager.Draw(new DrawElement
                {
                    drawAvfx = "general_1bxf",
                    Position = new Vector3(leftPos.X, 0f, leftPos.Z),
                    drawOnObject = false,
                    radiusX = 6f,
                    radiusZ = 6f,
                    delayDrawTime = 6900 + i * 1900,
                    destroyTime = 1900f
                }, (IGameObject?)Svc.Objects.LocalPlayer);
                Vector3 rightPos = position - RotateVector(new Vector3(7 * (i + 1), 0f, 0f), facing.Rad);
                DrawManager.Draw(new DrawElement
                {
                    drawAvfx = "general_1bxf",
                    Position = new Vector3(rightPos.X, 0f, rightPos.Z),
                    drawOnObject = false,
                    radiusX = 6f,
                    radiusZ = 6f,
                    delayDrawTime = 6900 + i * 1900,
                    destroyTime = 1900f
                }, (IGameObject?)Svc.Objects.LocalPlayer);
            }
        }
    }

    private static Vector3 RotateVector(Vector3 vector, float rotation)
    {
        float sin = MathF.Sin(rotation);
        float cos = MathF.Cos(rotation);
        return new Vector3(vector.X * cos + vector.Z * sin, z: (0f - vector.X) * sin + vector.Z * cos, y: vector.Y);
    }
}
