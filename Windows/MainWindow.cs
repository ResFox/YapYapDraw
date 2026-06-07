using System;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Windowing;

namespace YapYapDraw.Windows;

public sealed class MainWindow : Window, IDisposable
{
    private readonly HomeView       _home;
    private readonly LogWindow      _log;
    private readonly ModulesView    _modules;
    private readonly QuickDrawsView _quickDraws;
    private readonly LibraryView    _library;
    private readonly ActorMapWindow _map;
    private readonly ConfigWindow   _config;

    private string? _pendingTab;

    public MainWindow(Plugin plugin, LogWindow log, ConfigWindow config)
        : base("YapYap Draw###YapYapDrawMain")
    {
        _home       = new HomeView(plugin);
        _log        = log;
        _modules    = new ModulesView(plugin);
        _quickDraws = new QuickDrawsView(plugin);
        _library    = new LibraryView(plugin);
        _map        = new ActorMapWindow(plugin);
        _config     = config;

        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(740, 480),
            MaximumSize = new Vector2(2400, 2000),
        };
    }

    public void Dispose() => _map.Dispose();

    public override void PreDraw()  => Ui.PushTheme();
    public override void PostDraw() => Ui.PopTheme();

    public void Show(string tab)
    {
        _pendingTab = tab;
        IsOpen      = true;
        BringToFront();
    }

    public override void Draw()
    {
        if (!ImGui.BeginTabBar("##yaptabs", ImGuiTabBarFlags.None))
            return;

        Tab("Home",        "home",       _home.Draw);
        Tab("Modules",     "modules",    _modules.Draw);
        Tab("Quick Draws", "quickdraws", _quickDraws.Draw);
        Tab("Fight Log",   "log",        _log.DrawContent);
        Tab("Library",     "library",    _library.Draw);
        Tab("Live Map",    "map",        _map.Draw);
        Tab("Settings",    "settings",   _config.DrawContent);

        ImGui.EndTabBar();
        _pendingTab = null;
    }

    private void Tab(string label, string id, Action body)
    {
        var flags = ImGuiTabItemFlags.None;
        if (_pendingTab == id) flags |= ImGuiTabItemFlags.SetSelected;

        if (!ImGui.BeginTabItem($"{label}###tab_{id}", flags)) return;
        ImGui.Spacing();
        body();
        ImGui.EndTabItem();
    }
}
