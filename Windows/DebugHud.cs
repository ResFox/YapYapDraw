using System;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using YapYapDraw.Engine.Vfx;
using YapYapDraw.Logging;

namespace YapYapDraw.Windows;

public sealed class DebugHud
{
    private readonly Plugin _plugin;

    public DebugHud(Plugin plugin) => _plugin = plugin;

    public void Draw()
    {
        if (!_plugin.Configuration.DebugHud) return;

        var vp = ImGui.GetMainViewport();
        ImGui.SetNextWindowPos(vp.WorkPos + new Vector2(12, 12), ImGuiCond.FirstUseEver);
        ImGui.SetNextWindowBgAlpha(0.85f);

        const ImGuiWindowFlags flags = ImGuiWindowFlags.NoNav | ImGuiWindowFlags.AlwaysAutoResize;
        if (!ImGui.Begin("YapYap Draw debug", flags)) { ImGui.End(); return; }

        var cap  = _plugin.Capture;
        var host = _plugin.Host;

        ImGui.TextColored(new Vector4(1f, 0.85f, 0.3f, 1f), $"Build: {BuildStamp}");
        ImGui.TextUnformatted($"Active fight: {host.FightName}");
        ImGui.TextUnformatted($"Territory: {host.TerritoryId}   modules: {host.ModuleCount}");
        ImGui.TextUnformatted($"Hooks: casts {cap.ActionEffectInstalled}   control {cap.ActorControlInstalled}   mapfx {cap.MapEffectInstalled}   tether {VfxContainerHooks.Installed}");
        if (!string.IsNullOrEmpty(cap.InstallError))
            ImGui.TextColored(new Vector4(1, 0.4f, 0.4f, 1), cap.InstallError);
        if (!VfxContainerHooks.Installed && !string.IsNullOrEmpty(VfxContainerHooks.InstallError))
            ImGui.TextColored(new Vector4(1, 0.4f, 0.4f, 1), $"tether err: {VfxContainerHooks.InstallError}");
        ImGui.TextUnformatted($"Capturing now: {cap.ShouldCapture()}");
        ImGui.TextUnformatted($"Actors tracked: {cap.ActorsTracked}");

        ImGui.Separator();
        ImGui.TextUnformatted($"Memory events: {cap.TotalEmitted}  ({Ago(cap.LastEventAt)})");
        ImGui.TextUnformatted($"  casts {cap.KindCount(LogKind.CastStart)}   abilities {cap.KindCount(LogKind.Ability)}");
        ImGui.TextUnformatted($"  status+ {cap.KindCount(LogKind.StatusGain)}   status- {cap.KindCount(LogKind.StatusLose)}   deaths {cap.KindCount(LogKind.Death)}");
        ImGui.TextUnformatted($"  headmarkers {cap.KindCount(LogKind.Headmarker)}   tethers {cap.KindCount(LogKind.Tether)}");
        var mapFx = cap.KindCount(LogKind.MapEffect);
        ImGui.TextColored(mapFx > 0 ? new Vector4(0.4f, 0.9f, 0.4f, 1f) : new Vector4(1f, 0.6f, 0.3f, 1f),
            $"  mapeffects {mapFx}");
        if (!string.IsNullOrEmpty(cap.MapEffectError))
            ImGui.TextColored(new Vector4(1f, 0.4f, 0.4f, 1f), $"  mapfx err: {cap.MapEffectError}");
        if (!string.IsNullOrEmpty(cap.RecentMapEffects))
            ImGui.TextWrapped($"  mapfx: {cap.RecentMapEffects}");

        ImGui.End();
    }

    private static readonly string BuildStamp = GetBuildStamp();
    private static string GetBuildStamp()
    {
        try
        {
            var loc = Plugin.PluginInterface.AssemblyLocation.FullName;
            if (!string.IsNullOrEmpty(loc) && System.IO.File.Exists(loc))
                return System.IO.File.GetLastWriteTime(loc).ToString("HH:mm:ss");
            return "dev";
        }
        catch { return "?"; }
    }

    private static string Ago(DateTime t)
    {
        if (t == DateTime.MinValue) return "never";
        var s = (DateTime.Now - t).TotalSeconds;
        return s < 1 ? "now" : $"{s:0}s ago";
    }
}
