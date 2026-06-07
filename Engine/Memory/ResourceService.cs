using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using Dalamud.Hooking;
using YapYapDraw.Engine.Interop;
using FFXIVClientStructs.FFXIV.Client.System.Resource;
using FFXIVClientStructs.FFXIV.Client.System.Resource.Handle;
using InteropGenerator.Runtime;
using Penumbra.String;
using Penumbra.String.Classes;
using YapYapDraw.Engine.Managers;

namespace YapYapDraw.Engine.Memory;

public sealed class ResourceService : IDisposable
{
    private const string SigGetResourceSync  = "E8 ?? ?? ?? ?? 48 8B C8 8B C3 F0 0F C0 81";
    private const string SigGetResourceAsync = "E8 ?? ?? ?? 00 48 8B D8 EB ?? F0 FF 83 ?? ?? 00 00";

    public unsafe delegate ResourceHandle* GetResourceSyncDelegate(
        ResourceManager* resourceManager,
        ResourceCategory* category,
        uint* type,
        uint* hash,
        CStringPointer path,
        void* unk,
        void* unkDebugPtr,
        uint unkDebugInt);

    public unsafe delegate ResourceHandle* GetResourceAsyncDelegate(
        ResourceManager* resourceManager,
        ResourceCategory* category,
        uint* type,
        uint* hash,
        CStringPointer path,
        void* unk,
        bool isUnknown,
        void* unkDebugPtr,
        uint unkDebugInt);

    private sealed class Crc32
    {
        private readonly uint[] _table = Enumerable.Range(0, 256).Select(i =>
        {
            uint crc = (uint)i;
            for (var j = 0; j < 8; j++)
                crc = (crc & 1) != 0 ? (crc >> 1) ^ 0xEDB88320u : crc >> 1;
            return crc;
        }).ToArray();

        private uint _crc = uint.MaxValue;

        public uint Checksum => ~_crc;

        public void Init() => _crc = uint.MaxValue;

        public void Update(byte b) => _crc = _table[(_crc ^ b) & 0xFF] ^ (_crc >> 8);
    }

    private Crc32? _crc32;
    private Hook<GetResourceSyncDelegate>? _syncHook;
    private Hook<GetResourceAsyncDelegate>? _asyncHook;

    public unsafe void Init()
    {
        _syncHook = Svc.Hook.HookFromAddress<GetResourceSyncDelegate>(
            Svc.SigScanner.ScanText(SigGetResourceSync),
            SyncDetour);
        _syncHook.Enable();

        _asyncHook = Svc.Hook.HookFromAddress<GetResourceAsyncDelegate>(
            Svc.SigScanner.ScanText(SigGetResourceAsync),
            AsyncDetour);
        _asyncHook.Enable();

        _crc32 = new Crc32();
    }

    public void Dispose()
    {
        _syncHook?.Dispose();
        _asyncHook?.Dispose();
        _syncHook = null;
        _asyncHook = null;
    }

    private unsafe ResourceHandle* SyncDetour(
        ResourceManager* resourceManager, ResourceCategory* category, uint* type, uint* hash,
        CStringPointer path, void* unk, void* unkDebugPtr, uint unkDebugInt)
        => GetResourceHandler(true, resourceManager, category, type, hash, path, unk, false, unkDebugPtr, unkDebugInt);

    private unsafe ResourceHandle* AsyncDetour(
        ResourceManager* resourceManager, ResourceCategory* category, uint* type, uint* hash,
        CStringPointer path, void* unk, bool isUnknown, void* unkDebugPtr, uint unkDebugInt)
        => GetResourceHandler(false, resourceManager, category, type, hash, path, unk, isUnknown, unkDebugPtr, unkDebugInt);

    private unsafe ResourceHandle* GetResourceHandler(
        bool isSync, ResourceManager* resourceManager, ResourceCategory* category, uint* type, uint* hash,
        byte* path, void* unknown, bool isUnknown, void* unkDebugPtr, uint unkDebugInt)
    {
        if (!Utf8GamePath.FromPointer(path, MetaDataComputation.CiCrc32, out var gamePath))
        {
            if (!isSync)
                return _asyncHook!.Original(resourceManager, category, type, hash, path, unknown, isUnknown, unkDebugPtr, unkDebugInt);
            return _syncHook!.Original(resourceManager, category, type, hash, path, unknown, unkDebugPtr, unkDebugInt);
        }

        var text = gamePath.ToString();
        if (!VfxBlocker.BlockedPaths.Contains(text))
        {
            if (!isSync)
                return _asyncHook!.OriginalDisposeSafe(ResourceManager.Instance(), category, type, hash, path, unknown, isUnknown, unkDebugPtr, unkDebugInt);
            return _syncHook!.OriginalDisposeSafe(ResourceManager.Instance(), category, type, hash, path, unknown, unkDebugPtr, unkDebugInt);
        }

        var bytes = Encoding.ASCII.GetBytes("vfx/path/nothing.avfx");
        var buffer = stackalloc byte[bytes.Length + 1];
        Marshal.Copy(bytes, 0, (IntPtr)buffer, bytes.Length);
        path = buffer;
        _crc32!.Init();
        foreach (var b in bytes)
            _crc32.Update(b);
        *hash = _crc32.Checksum;

        if (!isSync)
            return _asyncHook!.OriginalDisposeSafe(ResourceManager.Instance(), category, type, hash, path, unknown, isUnknown, unkDebugPtr, unkDebugInt);
        return _syncHook!.OriginalDisposeSafe(ResourceManager.Instance(), category, type, hash, path, unknown, unkDebugPtr, unkDebugInt);
    }
}
