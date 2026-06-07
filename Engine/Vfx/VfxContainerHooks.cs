using System;
using Dalamud.Hooking;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using YapYapDraw.Logging;

namespace YapYapDraw.Engine.Vfx;


internal static class VfxContainerHooks
{
    private const uint InvalidTargetOid = 0xE0000000;

    private unsafe delegate long TetherCreateDelegate(VfxContainer* container, byte a2, ushort tetherId, ulong targetOid, byte a5);
    private unsafe delegate long TetherCancelDelegate(VfxContainer* container, byte a2, ushort a3, byte a4, byte a5);

    private static Hook<TetherCreateDelegate>? _createHook;
    private static Hook<TetherCancelDelegate>? _cancelHook;
    private static CombatLogCapture? _capture;

    private const string CreateSig =
        "48 89 5C 24 ?? 48 89 74 24 ?? 57 41 54 41 55 41 56 41 57 48 83 EC 20 0F B6 74 24";
    private const string CancelSig =
        "48 89 5C 24 ?? 48 89 6C 24 ?? 48 89 74 24 ?? 57 48 83 EC 20 0F B6 F2 41 0F B6 E9";

    public static bool Installed { get; private set; }
    public static string InstallError { get; private set; } = "";

    public unsafe static void Init(CombatLogCapture capture, IGameInteropProvider interop, ISigScanner sigScanner, IPluginLog log)
    {
        _capture = capture;
        try
        {
            _createHook = interop.HookFromAddress<TetherCreateDelegate>(
                sigScanner.ScanText(CreateSig), TetherCreateDetour);
            _createHook.Enable();

            _cancelHook = interop.HookFromAddress<TetherCancelDelegate>(
                sigScanner.ScanText(CancelSig), TetherCancelDetour);
            _cancelHook.Enable();

            Installed = true;
        }
        catch (Exception ex)
        {
            InstallError = ex.Message;
            log.Information($"[YapYapDraw] VfxContainer tether hooks unavailable on this build: {ex.Message}");
        }
    }

    public static void Dispose()
    {
        try { _createHook?.Dispose(); } catch { }
        try { _cancelHook?.Dispose(); } catch { }
        _createHook = null;
        _cancelHook = null;
        _capture = null;
        Installed = false;
    }

    private unsafe static long TetherCreateDetour(VfxContainer* container, byte a2, ushort tetherId, ulong targetOid, byte a5)
    {
        long result = _createHook!.Original(container, a2, tetherId, targetOid, a5);
        try
        {
            if (container == null) return result;
            var owner = container->OwnerObject;
            if (owner == null || targetOid == InvalidTargetOid) return result;

            _capture?.NotifyTetherFromVfx(owner->EntityId, (uint)targetOid, tetherId);
        }
        catch { }
        return result;
    }

    private unsafe static long TetherCancelDetour(VfxContainer* container, byte a2, ushort a3, byte a4, byte a5)
    {
        long result = _cancelHook!.Original(container, a2, a3, a4, a5);
        try
        {
            if (container == null) return result;
            var owner = container->OwnerObject;
            if (owner == null) return result;

            _capture?.NotifyTetherCancelFromVfx(owner->EntityId);
        }
        catch { }
        return result;
    }
}
