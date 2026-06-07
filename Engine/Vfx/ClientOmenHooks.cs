using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using YapYapDraw.Engine.Interop;
using YapYapDraw.Engine.Struct.Vfx;
using AddonEventDelegate = Dalamud.Plugin.Services.IAddonLifecycle.AddonEventDelegate;


namespace YapYapDraw.Engine.Vfx;

internal static class ClientOmenHooks
{
    public delegate nint CreateStaticVfxDelegate(string path, string pool);

    public delegate nint RunStaticVfxDelegate(nint vfx, float a1, uint a2);

    public delegate nint CreateActorVfxDelegate(string path, nint a2, nint a3, float a4, char a5, ushort a6, char a7);

    public delegate nint RemoveActorVfxDelegate(nint vfx, char a2);

    public delegate nint GetResourceDelegate(nint pFileManager, nint pCategoryId, nint pResourceType, nint pResourceHash, nint pPath, nint pUnknown);

    public delegate nint LoadResourceDelegate(nint pVfx, nint pFile, uint fileSize, nint pHandle);

    public delegate nint FinalizeResourceDelegate(nint pHandle);

    public delegate nint RemoveStaticVfxDelegate(nint vfxHandle, float time);

    public static CreateStaticVfxDelegate? createStaticVfx;

    public static RunStaticVfxDelegate? runStaticVfx;

    public static CreateActorVfxDelegate? createActorVfx;

    public static RemoveActorVfxDelegate? removeActorVfx;

    public static GetResourceDelegate? getResource;

    public static LoadResourceDelegate? loadResource;

    public static FinalizeResourceDelegate? finalizeResource;

    public static RemoveStaticVfxDelegate? removeStaticVfx;

    public static nint ResourceManagerAddress = 0;

    internal static bool ScreenTextReceived;

    private static AddonEventDelegate? _screenTextHandler;

    public static List<StaticVfx> drawOmenElementList { get; set; } = new List<StaticVfx>();

    public static List<ActorVfx> ActorVfxList { get; set; } = new List<ActorVfx>();

    public static List<(nint, nint)> TrackedVfxHandles { get; set; } = new List<(nint, nint)>();

    public unsafe static void Init()
    {
        nint ptr = Svc.SigScanner.ScanText("E8 ?? ?? ?? ?? F3 0F 10 35 ?? ?? ?? ?? 48 89 43 08");
        nint ptr2 = Svc.SigScanner.ScanText("e8 ?? ?? ?? ?? 0f ?? ?? ?? ?? ?? ?? 66 ?? ?? ?? 74 ??");
        nint ptr3 = Svc.SigScanner.ScanText("40 53 55 56 57 48 81 EC ?? ?? ?? ?? 0F 29 B4 24 ?? ?? ?? ?? 48 8B 05 ?? ?? ?? ?? 48 33 C4 48 89 84 24 ?? ?? ?? ?? 0F B6 AC 24 ?? ?? ?? ?? 0F 28 F3 49 8B F8");
        nint addr = (nint)Svc.SigScanner.ScanText("0F 11 48 10 48 8D 05") + 7;
        nint ptr4 = Marshal.ReadIntPtr(addr + Marshal.ReadInt32(addr) + 4);
        nint ptr5 = Svc.SigScanner.ScanText("83 89 ?? ?? ?? ?? ?? f3 0f 11 89");
        createStaticVfx = Marshal.GetDelegateForFunctionPointer<CreateStaticVfxDelegate>(ptr);
        runStaticVfx = Marshal.GetDelegateForFunctionPointer<RunStaticVfxDelegate>(ptr2);
        createActorVfx = Marshal.GetDelegateForFunctionPointer<CreateActorVfxDelegate>(ptr3);
        removeActorVfx = Marshal.GetDelegateForFunctionPointer<RemoveActorVfxDelegate>(ptr4);
        removeStaticVfx = Marshal.GetDelegateForFunctionPointer<RemoveStaticVfxDelegate>(ptr5);
        ResourceManagerAddress = (nint)((IntPtr)(nint)Svc.SigScanner.ScanText("48 ?? ?? ?? ?? ?? ?? f0 0f c1 8a ?? ?? ?? ??")).ToPointer();
        IntPtr ptr6 = Svc.SigScanner.ScanText("48 89 5c 24 ?? 48 89 6c 24 ?? 48 89 74 24 ?? 57 41 ?? 41 ?? 41 ?? 41 ?? 48 ?? ?? ?? 48 ?? ?? 49 ?? ?? 48 ?? ?? ??");
        nint ptr7 = Svc.SigScanner.ScanText("48 89 5c 24 ?? 48 89 6c 24 ?? 48 89 74 24 ?? 48 89 7c 24 ?? 41 ?? 48 ?? ?? ?? 48 ?? ?? ?? 49 ?? ?? 48 ?? ?? ?? 41 ?? ??");
        nint ptr8 = Svc.SigScanner.ScanText("40 ?? 48 ?? ?? ?? 48 ?? ?? 33 ?? 8b ?? f0 0f c0 83");
        getResource = Marshal.GetDelegateForFunctionPointer<GetResourceDelegate>(ptr6);
        loadResource = Marshal.GetDelegateForFunctionPointer<LoadResourceDelegate>(ptr7);
        finalizeResource = Marshal.GetDelegateForFunctionPointer<FinalizeResourceDelegate>(ptr8);
        AddonEventDelegate handler = _screenTextHandler ??= OnScreenTextPostReceive;
        Svc.AddonLifecycle.RegisterListener((AddonEvent)5, "_ScreenText", handler);
    }

    private static void OnScreenTextPostReceive(AddonEvent type, AddonArgs args)
    {
        ScreenTextReceived = true;
    }

    public static void CleanAllVfx()
    {
        foreach (StaticVfx item in drawOmenElementList.ToList())
        {
            item.Remove();
        }
        foreach (ActorVfx item2 in ActorVfxList.ToList())
        {
            item2.Remove();
        }
        Svc.Framework.RunOnTick((Action)delegate
        {
            foreach (var item3 in TrackedVfxHandles)
            {
                if (VFXList.CheckVFXHandleExists(item3.Item2))
                {
                    removeStaticVfx(item3.Item1, 10f);
                }
            }
            TrackedVfxHandles.Clear();
        }, default, 0, default);
    }

    public static void DisposeHooks()
    {
        CleanAllVfx();
        Svc.AddonLifecycle.UnregisterListener(Array.Empty<AddonEventDelegate>());
    }
}
