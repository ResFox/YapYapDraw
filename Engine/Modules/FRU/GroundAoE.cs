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

namespace YapYapDraw.Modules.FRU;

public class GroundAoE : ISpecialAction
{
    public override string Name => "Ground AoE";

    public override uint Phase => 5u;

    public override HashSet<uint> ActionID => new HashSet<uint> { 40118u, 40307u };

    public override bool HasConfig => true;

    public override void DrawConfig()
    {
        ImGui.SetNextItemWidth(300f);
        var color = YapYapDraw.Plugin.Config.FruP5HellfireColor;
        if (ImGui.ColorEdit4("Hellfire color", ref color, (ImGuiColorEditFlags)0))
        {
            YapYapDraw.Plugin.Config.FruP5HellfireColor = color;
            YapYapDraw.Plugin.Config.Save();
        }
        ImGui.SameLine();
        if (ImGui.Button("Preview color", default))
        {
            DrawManager.Draw(new DrawElement
            {
                drawAvfx = "customRect",
                drawOnObject = true,
                radiusX = 40f,
                radiusZ = 5f,
                destroyTime = 3000f,
                refColor = YapYapDraw.Plugin.Config.FruP5HellfireColor,
                refTargetColor = YapYapDraw.Plugin.Config.FruP5HellfireColor
            }, (IGameObject?)Svc.Objects.LocalPlayer);
        }
        ImGui.SameLine();
        if (ImGuiUtil.IconButton((FontAwesomeIcon)61470, "Reset###FruP5HellfireColor"))
        {
            YapYapDraw.Plugin.Config.FruP5HellfireColor = new Vector4(1f, 1f, 1f, 2f);
            YapYapDraw.Plugin.Config.Save();
        }
        ImGui.Text("If it looks faint, double-click the 4th (A) value to type a number above 255 for noticeably brighter drawing.");
    }

    public override void OnActionCast(ActorCastInfo info)
    {
        SimpleElement.LineRect(info, 5f, 2000f, 8, ShowAll: false, YapYapDraw.Plugin.Config.FruP5HellfireColor);
    }
}
