using System;
using System.Collections.Generic;
using System.Numerics;
using Dalamud.Game.ClientState.Objects.Types;
using YapYapDraw.Engine.Interop;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Enum;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.ModuleSetup;

namespace YapYapDraw.Modules.Everkeep;

public class ProjectionOfTriumph : ISpecialAction
{
    public override string Name => "Projection of Triumph (donut)";

    public override uint Phase => 3u;

    public override HashSet<uint> ActionID => new HashSet<uint>();

    public override void OnObjectCreatedEvent(IGameObject GameObject)
    {
        if (GameObject.BaseId == 16726)
        {
            float crystalRot = GameObject.Rotation;
            Vector2 crystalFwd = new Vector2(MathF.Sin(crystalRot), MathF.Cos(crystalRot));
            for (int i = 0; i < 4; i++)
            {
                Vector2 lineCenter = new Vector2(100f, 100f) + (-15 + 10 * i) * crystalFwd;
                Vector2 perpendicular = new Vector2(crystalFwd.Y, 0f - crystalFwd.X);
                for (int j = -15; j <= 15; j += 10)
                {
                    DrawManager.Draw(new DrawElement
                    {
                        drawAvfx = "customCircle",
                        Position = new Vector3(lineCenter.X + (float)j * perpendicular.X, 0f, lineCenter.Y + (float)j * perpendicular.Y),
                        drawOnObject = false,
                        radiusX = 4f,
                        radiusZ = 4f,
                        destroyTime = ((i == 0) ? 9000 : 5000),
                        delayDrawTime = ((i != 0) ? (9000 + 5000 * (i - 1)) : 0),
                        refColor = new Vector4(1f, 1f, 1f, 0.1f),
                        refTargetColor = GroundOmen.enemyColor
                    }, (IGameObject?)Svc.Objects.LocalPlayer);
                }
            }
        }
        if (GameObject.BaseId != 16727)
        {
            return;
        }
        float rotation = GameObject.Rotation;
        Vector2 forward = new Vector2(MathF.Sin(rotation), MathF.Cos(rotation));
        for (int i = 0; i < 4; i++)
        {
            Vector2 lineCenter = new Vector2(100f, 100f) + (-15 + 10 * i) * forward;
            Vector2 perpendicular = new Vector2(forward.Y, 0f - forward.X);
            for (int j = -15; j <= 15; j += 10)
            {
                DrawManager.Draw(new DrawElement
                {
                    drawAvfx = "customDonut",
                    Position = new Vector3(lineCenter.X + (float)j * perpendicular.X, 0f, lineCenter.Y + (float)j * perpendicular.Y),
                    drawOnObject = false,
                    radiusX = 8f,
                    radiusZ = 8f,
                    refRadian = 0.375f,
                    destroyTime = ((i == 0) ? 9000 : 5000),
                    delayDrawTime = ((i != 0) ? (9000 + 5000 * (i - 1)) : 0),
                    refColor = new Vector4(1f, 1f, 1f, 0.1f),
                    refTargetColor = GroundOmen.enemyColor
                }, (IGameObject?)Svc.Objects.LocalPlayer);
            }
        }
    }
}
