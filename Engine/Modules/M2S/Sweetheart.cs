using System.Collections.Generic;
using Dalamud.Bindings.ImGui;
using Dalamud.Game.ClientState.Objects.Types;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.ModuleSetup;

namespace YapYapDraw.Modules.M2S;

public class Sweetheart : ISpecialAction
{
    public override string Name => "Sweetheart";

    public override uint Phase => 1u;

    public override HashSet<uint> ActionID => new HashSet<uint>();

    public override bool HasConfig => true;

    public override void DrawConfig()
    {
        ImGui.Text("Draw mode:");
        ImGui.SameLine();
        if (ImGui.RadioButton("Short line", YapYapDraw.Plugin.Config.M2SSweetheartDrawMode == 0))
        {
            YapYapDraw.Plugin.Config.M2SSweetheartDrawMode = 0;
            YapYapDraw.Plugin.Config.Save();
        }
        ImGui.SameLine();
        if (ImGui.RadioButton("Long line", YapYapDraw.Plugin.Config.M2SSweetheartDrawMode == 1))
        {
            YapYapDraw.Plugin.Config.M2SSweetheartDrawMode = 1;
            YapYapDraw.Plugin.Config.Save();
        }
    }

    public override void OnActorPlayActionTimelineEvent(IGameObject source, uint id)
    {
        if (id == 4563 && source.BaseId == 16943)
        {
            DrawManager.Draw(new DrawElement
            {
                drawAvfx = "general02xf",
                radiusX = 1.5f,
                radiusZ = ((YapYapDraw.Plugin.Config.M2SSweetheartDrawMode == 0) ? 3 : 100),
                OnlyVisible = true,
                hitCounter = new HitCounter
                {
                    ActionID = new HashSet<uint> { 37263u }
                }
            }, source);
            DrawManager.Draw(new DrawElement
            {
                drawAvfx = "general_1bxf",
                radiusX = 1.5f,
                radiusZ = 1.5f,
                OnlyVisible = true,
                hitCounter = new HitCounter
                {
                    ActionID = new HashSet<uint> { 37263u }
                }
            }, source);
        }
    }
}
