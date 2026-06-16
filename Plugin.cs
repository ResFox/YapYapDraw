using System;
using System.IO;
using Dalamud.Game;
using Dalamud.Game.Command;
using Dalamud.Interface.ManagedFontAtlas;
using Dalamud.Interface.Windowing;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Game.Chat;
using Dalamud.Plugin.Services;
using YapYapDraw.Logging;
using YapYapDraw.Engine;
using YapYapDraw.QuickDraws;
using YapYapDraw.Strats;
using YapYapDraw.Windows;

namespace YapYapDraw;

public sealed class Plugin : IDalamudPlugin
{
    internal static Plugin? Instance { get; private set; }
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

    private static Lumina.Excel.ExcelSheet<Lumina.Excel.Sheets.Action>? _actionsEn;
    private static Lumina.Excel.ExcelSheet<Lumina.Excel.Sheets.Status>? _statusesEn;

    internal static Lumina.Excel.ExcelSheet<Lumina.Excel.Sheets.Action> Actions
        => _actionsEn ??= DataManager.GetExcelSheet<Lumina.Excel.Sheets.Action>(ClientLanguage.English);
    internal static Lumina.Excel.ExcelSheet<Lumina.Excel.Sheets.Status> Statuses
        => _statusesEn ??= DataManager.GetExcelSheet<Lumina.Excel.Sheets.Status>(ClientLanguage.English);

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
    public StratEngine      Strat         { get; }

    public readonly WindowSystem WindowSystem = new("YapYapDraw");
    private readonly MainWindow   _mainWindow;
    private readonly LogWindow    _logWindow;
    private readonly ConfigWindow _configWindow;
    private readonly ModuleConfigWindow _moduleConfigWindow;
    private readonly QuickDrawEditorWindow _quickDrawEditor;
    private readonly StratEditorWindow _stratEditor;
    private readonly ChangelogWindow _changelogWindow;
    private readonly DebugHud     _debugHud;
    private readonly LabelOverlay _labelOverlay;
    private readonly ArrowOverlay _arrowOverlay;
    private readonly IFontHandle  _labelFont;
    public IFontHandle            LabelFont => _labelFont;

    public Plugin()
    {
        Instance = this;
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
        Engine  = new QuickDrawEngine(Configuration, Log, Capture);
        Catalog = new FightCatalog(PluginInterface.GetPluginConfigDirectory(), Log);
        Strat   = new StratEngine(Configuration, Engine, Log, Capture);

        Capture.OnEvent   += Host.OnEvent;
        Capture.OnEvent   += Engine.Handle;
#if DEBUG
        Capture.OnEvent   += Strat.Handle;
#endif
        Capture.OnEvent   += Catalog.Record;
        Capture.OnNpcYell += Host.HandleNpcYell;
        ChatGui.ChatMessage += OnChatMessage;

        DutyState.DutyWiped      += OnDutyWiped;
        DutyState.DutyRecommenced += OnDutyRecommenced;
        DutyState.DutyCompleted  += OnDutyCompleted;

        _labelFont = PluginInterface.UiBuilder.FontAtlas.NewDelegateFontHandle(e =>
            e.OnPreBuild(tk =>
            {
                var fontsDir = Environment.ExpandEnvironmentVariables(@"%SystemRoot%\Fonts");
                var cfg      = new SafeFontConfig { SizePx = 72f };
                foreach (var file in new[] { "segoeuib.ttf", "arialbd.ttf", "ariblk.ttf", "tahomabd.ttf", "verdanab.ttf" })
                {
                    var path = Path.Combine(fontsDir, file);
                    if (File.Exists(path)) { tk.AddFontFromFile(path, cfg); return; }
                }
                tk.AddDalamudDefaultFont(72f);
            }));

        _debugHud      = new DebugHud(this);
        _labelOverlay  = new LabelOverlay(this);
        _arrowOverlay  = new ArrowOverlay(this);
        _logWindow     = new LogWindow(this);
        _configWindow  = new ConfigWindow(this);
        _quickDrawEditor = new QuickDrawEditorWindow(this);
        _stratEditor   = new StratEditorWindow(this);
        _mainWindow    = new MainWindow(this, _logWindow, _configWindow);
        _moduleConfigWindow = new ModuleConfigWindow();
        _changelogWindow = new ChangelogWindow(this);

        WindowSystem.AddWindow(_mainWindow);
        WindowSystem.AddWindow(_moduleConfigWindow);
        WindowSystem.AddWindow(_quickDrawEditor);
        WindowSystem.AddWindow(_stratEditor);
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
        PluginInterface.UiBuilder.Draw         += _arrowOverlay.Draw;
        PluginInterface.UiBuilder.Draw         += _labelOverlay.Draw;
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
#if DEBUG
        Capture.OnEvent   -= Strat.Handle;
#endif
        Capture.OnEvent   -= Catalog.Record;
        Capture.OnNpcYell -= Host.HandleNpcYell;
        DutyState.DutyWiped      -= OnDutyWiped;
        DutyState.DutyRecommenced -= OnDutyRecommenced;
        DutyState.DutyCompleted  -= OnDutyCompleted;
        try { Catalog.Save(); } catch (Exception ex) { Log.Debug($"[YapYapDraw] catalog save: {ex.Message}"); }

        PluginInterface.UiBuilder.Draw         -= WindowSystem.Draw;
        PluginInterface.UiBuilder.Draw         -= _debugHud.Draw;
        PluginInterface.UiBuilder.Draw         -= _arrowOverlay.Draw;
        PluginInterface.UiBuilder.Draw         -= _labelOverlay.Draw;
        PluginInterface.UiBuilder.OpenConfigUi -= ToggleConfig;
        PluginInterface.UiBuilder.OpenMainUi -= ToggleMain;
        ClientState.Login -= OnLogin;
        WindowSystem.RemoveAllWindows();

        try { _labelFont.Dispose(); } catch (Exception ex) { Log.Debug($"[YapYapDraw] label font dispose: {ex.Message}"); }

        Assets.Dispose();

        try { Host.Dispose(); } catch (Exception ex) { Log.Debug($"[YapYapDraw] host dispose: {ex.Message}"); }
        try { Capture.Dispose(); } catch (Exception ex) { Log.Debug($"[YapYapDraw] capture dispose: {ex.Message}"); }

        CommandManager.RemoveHandler(CmdMain);
        CommandManager.RemoveHandler(CmdShort);
        Instance = null;
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
#if DEBUG
        Strat.Tick();
#endif
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

    public void OpenStrat(StratPack pack) => _stratEditor.Open(pack);

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
