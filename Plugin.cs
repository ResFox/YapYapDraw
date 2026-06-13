using System;
using Dalamud.Game;
using Dalamud.Game.Command;
using Dalamud.Interface.Windowing;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Game.Chat;
using Dalamud.Plugin.Services;
using YapYapDraw.Logging;
using YapYapDraw.Engine;
using YapYapDraw.QuickDraws;
using YapYapDraw.Windows;

namespace YapYapDraw;

public sealed class Plugin : IDalamudPlugin
{
    [PluginService] internal static IDalamudPluginInterface PluginInterface { get; private set; } = null!;
    [PluginService] internal static ICommandManager      CommandManager  { get; private set; } = null!;
    [PluginService] internal static IClientState         ClientState     { get; private set; } = null!;
    [PluginService] internal static IPlayerState         PlayerState     { get; private set; } = null!;
    [PluginService] internal static IObjectTable         ObjectTable     { get; private set; } = null!;
    [PluginService] internal static IFramework           Framework       { get; private set; } = null!;
    [PluginService] internal static IDataManager         DataManager     { get; private set; } = null!;
    [PluginService] internal static IDutyState           DutyState       { get; private set; } = null!;
    [PluginService] internal static ICondition           Condition       { get; private set; } = null!;
    [PluginService] internal static IGameInteropProvider GameInterop     { get; private set; } = null!;
    [PluginService] internal static ISigScanner          SigScanner      { get; private set; } = null!;
    [PluginService] internal static IAddonLifecycle      AddonLifecycle  { get; private set; } = null!;
    [PluginService] internal static ITextureProvider     TextureProvider { get; private set; } = null!;
    [PluginService] internal static IGameGui              GameGui         { get; private set; } = null!;
    [PluginService] internal static IPartyList           PartyList       { get; private set; } = null!;
    [PluginService] internal static IChatGui             ChatGui         { get; private set; } = null!;
    [PluginService] internal static IPluginLog           Log             { get; private set; } = null!;

    private const string CmdMain  = "/yapdraw";
    private const string CmdShort = "/yd";

    internal static Configuration? ConfigStatic;
    public static void DebugLog(string message) => Log?.Debug(message);
    public static void DebugChat(string message) => Log?.Info($"[YapYapDraw] {message}");
    public static void Chat(string message) => DebugLog(message);
    public Configuration Configuration { get; private set; } = null!;
    public static Configuration Config => ConfigStatic!;
    public CombatLogCapture Capture       { get; }
    public FightModuleHost  Host          { get; }
    public QuickDrawEngine  Engine        { get; }
    public FightCatalog     Catalog       { get; }

    public readonly WindowSystem WindowSystem = new("YapYapDraw");
    private readonly MainWindow   _mainWindow;
    private readonly LogWindow    _logWindow;
    private readonly ConfigWindow _configWindow;
    private readonly ModuleConfigWindow _moduleConfigWindow;
    private readonly QuickDrawEditorWindow _quickDrawEditor;
    private readonly ChangelogWindow _changelogWindow;
    private readonly DebugHud     _debugHud;

    public Plugin()
    {
        Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
        if (Configuration.CustomAlpha < 1f) Configuration.CustomAlpha = 1.5f;
        foreach (var m in Configuration.QuickDrawModules)
            foreach (var d in m.Draws)
            {
                d.Draw.NormalizeLegacy();
                foreach (var s in d.FollowUps) s.Draw.NormalizeLegacy();
            }
        ConfigStatic  = Configuration;

        Capture = new CombatLogCapture(Configuration, GameInterop, Log);
        Host    = new FightModuleHost(Log, Capture);
        Engine  = new QuickDrawEngine(Configuration, Log);
        Catalog = new FightCatalog(PluginInterface.GetPluginConfigDirectory(), Log);

        Capture.OnEvent   += Host.OnEvent;
        Capture.OnEvent   += Engine.Handle;
        Capture.OnEvent   += Catalog.Record;
        Capture.OnNpcYell += Host.HandleNpcYell;
        ChatGui.ChatMessage += OnChatMessage;

        DutyState.DutyWiped      += OnDutyWiped;
        DutyState.DutyRecommenced += OnDutyRecommenced;
        DutyState.DutyCompleted  += OnDutyCompleted;

        _debugHud      = new DebugHud(this);
        _logWindow     = new LogWindow(this);
        _configWindow  = new ConfigWindow(this);
        _quickDrawEditor = new QuickDrawEditorWindow(this);
        _mainWindow    = new MainWindow(this, _logWindow, _configWindow);
        _moduleConfigWindow = new ModuleConfigWindow();
        _changelogWindow = new ChangelogWindow(this);

        WindowSystem.AddWindow(_mainWindow);
        WindowSystem.AddWindow(_moduleConfigWindow);
        WindowSystem.AddWindow(_quickDrawEditor);
        WindowSystem.AddWindow(_changelogWindow);

        if (!Configuration.FirstRun && Configuration.LastSeenVersion != Changelog.Version)
            _changelogWindow.IsOpen = true;
        else if (Configuration.LastSeenVersion != Changelog.Version)
        {
            Configuration.LastSeenVersion = Changelog.Version;
            Configuration.Save();
        }

        CommandManager.AddHandler(CmdMain, new CommandInfo(OnCommand)
        {
            ShowInHelp = false,
        });

        CommandManager.AddHandler(CmdShort, new CommandInfo(OnCommand)
        {
            HelpMessage = "Open YapYap Draw.\n/yd modules\n/yd config\n/yd clean",
        });

        PluginInterface.UiBuilder.Draw         += WindowSystem.Draw;
        PluginInterface.UiBuilder.Draw         += _debugHud.Draw;
        PluginInterface.UiBuilder.OpenConfigUi += ToggleConfig;
        PluginInterface.UiBuilder.OpenMainUi   += ToggleMain;

        ClientState.Login += OnLogin;

        Framework.Update += OnFrameworkUpdate;
    }

    private void OnLogin()
    {
        if (Configuration.OpenOnLogin)
            _mainWindow.Show("home");
    }

    private void OnDutyWiped(Dalamud.Game.DutyState.IDutyStateEventArgs args)      => Host.HandleDutyWipe();
    private void OnDutyRecommenced(Dalamud.Game.DutyState.IDutyStateEventArgs args) => Host.HandleDutyWipe();
    private void OnDutyCompleted(Dalamud.Game.DutyState.IDutyStateEventArgs args)  => Host.HandleDutyWipe();

    public void Dispose()
    {
        Framework.Update -= OnFrameworkUpdate;
        ChatGui.ChatMessage -= OnChatMessage;
        Capture.OnEvent   -= Host.OnEvent;
        Capture.OnEvent   -= Engine.Handle;
        Capture.OnEvent   -= Catalog.Record;
        Capture.OnNpcYell -= Host.HandleNpcYell;
        DutyState.DutyWiped      -= OnDutyWiped;
        DutyState.DutyRecommenced -= OnDutyRecommenced;
        DutyState.DutyCompleted  -= OnDutyCompleted;
        try { Catalog.Save(); } catch (Exception ex) { Log.Debug($"[YapYapDraw] catalog save: {ex.Message}"); }

        PluginInterface.UiBuilder.Draw         -= WindowSystem.Draw;
        PluginInterface.UiBuilder.Draw         -= _debugHud.Draw;
        PluginInterface.UiBuilder.OpenConfigUi -= ToggleConfig;
        PluginInterface.UiBuilder.OpenMainUi -= ToggleMain;
        ClientState.Login -= OnLogin;
        WindowSystem.RemoveAllWindows();

        Assets.Dispose();

        try { Host.Dispose(); } catch (Exception ex) { Log.Debug($"[YapYapDraw] host dispose: {ex.Message}"); }
        try { Capture.Dispose(); } catch (Exception ex) { Log.Debug($"[YapYapDraw] capture dispose: {ex.Message}"); }

        CommandManager.RemoveHandler(CmdMain);
        CommandManager.RemoveHandler(CmdShort);
    }

    private void OnFrameworkUpdate(IFramework framework){
        bool inCombat = Condition[Dalamud.Game.ClientState.Conditions.ConditionFlag.InCombat];
        Capture.NotifyCombat(inCombat);

        if (Configuration.FirstRun)
        {
            Configuration.FirstRun = false;
            Configuration.Save();
        }

        Capture.Update();
        Host.Tick();
        Engine.Tick();
        Catalog.MaybeSave();
        _quickDrawEditor.TickGroundPick();
    }

    private void OnChatMessage(IHandleableChatMessage message)
    {
        if ((int)message.LogKind - 41 <= 8)
            return;
        Host.HandleChatMessage((uint)(int)message.LogKind, message.Message.TextValue);
    }

    private void OnCommand(string command, string args)
    {
        switch (args.Trim().ToLowerInvariant())
        {
            case "config":
            case "c":
            case "settings":
                ShowTab("settings");
                break;
            case "modules":
            case "m":
                ShowTab("modules");
                break;
            case "home":
                ShowTab("home");
                break;
            case "map":
            case "livemap":
                ShowTab("map");
                break;
            case "cleanvfx":
            case "clean":
                Host.CleanVfx();
                DebugChat("Cleared all drawn VFX.");
                break;
            default:
                ToggleMain();
                break;
        }
    }

    public void ShowTab(string tab) => _mainWindow.Show(tab);

    public void OpenChangelog() => _changelogWindow.IsOpen = true;

    public void OpenModuleConfig(string title, System.Action body) => _moduleConfigWindow.Open(title, body);

    public void OpenQuickDraw(QuickDrawDef def) => _quickDrawEditor.Open(def);
    public void OpenQuickDrawFor(LogEvent e)    => _quickDrawEditor.OpenFor(e);
    public void OpenQuickDrawForCatalog(FightCatalog.Entry entry, uint territory)
        => _quickDrawEditor.OpenForCatalog(entry, territory);

    public void ToggleMain()
    {
        if (_mainWindow.IsOpen) _mainWindow.IsOpen = false;
        else                    _mainWindow.Show("home");
    }

    public void ToggleLog()    => _mainWindow.Show("log");
    public void ToggleConfig() => _mainWindow.Show("settings");
    public void OpenConfig()   => _mainWindow.Show("settings");
}
