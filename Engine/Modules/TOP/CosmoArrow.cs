using System;
using System.Collections.Generic;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Interface;
using YapYapDraw.Engine.Interop;
using YapYapDraw.Engine.Interop.Ui;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;
using YapYapDraw.Engine.Util;

namespace YapYapDraw.Modules.TOP;

public class CosmoArrow : ISpecialAction
{
    public override string Name => "Cosmo Arrow";

    public override uint WeatherID => 175u;

    public override uint Phase => 6u;

    public override HashSet<uint> ActionID => new HashSet<uint> { 31651u };

    public override bool HasConfig => true;

    public override void DrawConfig()
    {
        ImGui.SetNextItemWidth(300f);
        var color = YapYapDraw.Plugin.Config.TopP6CosmoArrowColor;
        if (ImGui.ColorEdit4("P6 Cosmo Arrow", ref color, (ImGuiColorEditFlags)0))
        {
            YapYapDraw.Plugin.Config.TopP6CosmoArrowColor = color;
            YapYapDraw.Plugin.Config.Save();
        }
        ImGui.SameLine();
        if (ImGui.Button("Preview color", default))
        {
            DrawManager.Draw(new DrawElement
            {
                drawAvfx = "customRect2",
                drawOnObject = true,
                radiusX = 2.5f,
                radiusZ = 10f,
                destroyTime = 3000f,
                refColor = YapYapDraw.Plugin.Config.TopP6CosmoArrowColor,
                refTargetColor = YapYapDraw.Plugin.Config.TopP6CosmoArrowColor
            }, (IGameObject?)Svc.Objects.LocalPlayer);
        }
        ImGui.SameLine();
        if (ImGuiUtil.IconButton((FontAwesomeIcon)61470, "Reset###TopP6CosmoArrowColor"))
        {
            YapYapDraw.Plugin.Config.TopP6CosmoArrowColor = new Vector4(1f, 1f, 0f, 1f);
            YapYapDraw.Plugin.Config.Save();
        }
    }

    public override void OnActionCast(ActorCastInfo info)
    {
        IGameObject? source = info.SourceId.GameObject();
        Angle facing = info.Facing;
        Vector3 position = source.Position;
        float cos = (float)Math.Cos(facing.Rad);
        float sinFacing = (float)Math.Sin(facing.Rad);
        float cosFacing = cos;
        for (int i = 0; i < 6; i++)
        {
            float distance = 2.5f + (float)(5 * (i + 1));
            int delayTime = (int)(info.CastTime * 1000f) + 2000 * i;
            CreateDirectionalOmen(position, cosFacing, sinFacing, distance, facing, delayTime);
            CreateDirectionalOmen(position, cosFacing, sinFacing, 0f - distance, facing, delayTime);
        }
    }

    private void CreateDirectionalOmen(Vector3 basePosition, float cosFacing, float sinFacing, float offset, Angle facing, int delayTime)
    {
        Vector3 position = new Vector3(basePosition.X + cosFacing * offset, basePosition.Y, basePosition.Z + sinFacing * offset);
        DrawManager.Draw(new DrawElement
        {
            drawAvfx = "customRect",
            Position = position,
            drawOnObject = false,
            refRotation = facing,
            radiusX = 2.5f,
            radiusZ = 100f,
            destroyTime = 2000f,
            delayDrawTime = delayTime,
            refColor = YapYapDraw.Plugin.Config.TopP6CosmoArrowColor,
            refTargetColor = YapYapDraw.Plugin.Config.TopP6CosmoArrowColor
        }, (IGameObject?)Svc.Objects.LocalPlayer);
    }
}
