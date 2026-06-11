using System.Collections.Generic;
using Dalamud.Bindings.ImGui;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.ModuleSetup;

namespace YapYapDraw.Modules.DancingMad.P3;

public class UltimateShockwave : ISpecialAction
{
    public override string Name => "Ultimate Shockwave";

    public override uint Phase => 3u;

    public override HashSet<uint> ActionID => new HashSet<uint>();

    public override bool HasConfig => true;

    public override void DrawConfig()
    {
        bool block = Plugin.Config.DancingMadBlockShockwave;
        if (ImGui.Checkbox("Block fullscreen shockwave VFX", ref block))
        {
            Plugin.Config.DancingMadBlockShockwave = block;
            Plugin.Config.Save();
            VfxBlocker.ClearSyncedBlocks();
        }
        ImGui.TextDisabled("Hides the fullscreen white-out during the Ultimate Shockwave (weather 174).");
    }
}
