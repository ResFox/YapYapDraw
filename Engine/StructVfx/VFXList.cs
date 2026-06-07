using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using YapYapDraw.Engine.Interop;


namespace YapYapDraw.Engine.Struct.Vfx;

[StructLayout(LayoutKind.Explicit, Size = 278528)]
public struct VFXList : IEnumerable<VFXListData>, IEnumerable
{
    private static nint _getListFn = IntPtr.Zero;

    private static nint _scanAddress = IntPtr.Zero;

    private static int _listOffset = 0;

    private static bool _fnResolved;

    private static bool _addressScanned;

    private static bool _offsetResolved;

    public static HashSet<nint> vfxHandlesSet = new HashSet<nint>();

    [FieldOffset(0)]
    private unsafe fixed byte _buffer[278528];

    public unsafe Span<VFXListData> ListSpan
    {
        get
        {
            fixed (byte* pointer = _buffer)
            {
                return new Span<VFXListData>(pointer, 2048);
            }
        }
    }

    public static bool CheckVFXHandleExists(nint vfxHandle)
    {
        return vfxHandlesSet.Contains(vfxHandle);
    }

    public unsafe static void SyncVfxHandles()
    {
        vfxHandlesSet.Clear();
        VFXList* ptr = Instance();
        if (ptr == null)
        {
            return;
        }
        try
        {
            Span<VFXListData> listSpan = ptr->ListSpan;
            for (int i = 0; i < listSpan.Length; i++)
            {
                VFXListData entry = listSpan[i];
                if (entry.IsValid())
                {
                    vfxHandlesSet.Add(entry.VFXHandle);
                }
            }
        }
        catch (Exception e)
        {
            e.Log();
        }
    }

    public unsafe VFXListData* GetVFXListDataByIndex(int index)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(index, 0, "index");
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, 2048, "index");
        return (VFXListData*)Unsafe.AsPointer(in ListSpan[index]);
    }

    public unsafe static VFXList* Instance()
    {
        nint listPtr = GetListPointer();
        if (IsInvalidPointer(listPtr))
        {
            return null;
        }
        nint inner = *(nint*)(listPtr + ResolveListOffset());
        if (IsInvalidPointer(inner))
        {
            return null;
        }
        nint result = inner + 8192;
        if (IsInvalidPointer(result))
        {
            return null;
        }
        return (VFXList*)result;
    }

    private unsafe static nint GetListPointer()
    {
        ResolveFunction();
        if (IsInvalidPointer(_getListFn))
        {
            return IntPtr.Zero;
        }
        try
        {
            return ((delegate* unmanaged[Stdcall]<nint>)_getListFn)();
        }
        catch
        {
            return IntPtr.Zero;
        }
    }

    private unsafe static void ResolveFunction()
    {
        if (_fnResolved)
        {
            return;
        }
        _fnResolved = true;
        nint funcAddr = ScanFunction();
        if (IsInvalidPointer(funcAddr))
        {
            return;
        }
        try
        {
            byte* ptr = (byte*)funcAddr;
            if (ptr[6] == 232)
            {
                int relOffset = *(int*)(ptr + 7);
                nint targetAddr = funcAddr + 11 + relOffset;
                if (!IsInvalidPointer(targetAddr))
                {
                    _getListFn = targetAddr;
                }
            }
        }
        catch
        {
            _getListFn = IntPtr.Zero;
        }
    }

    private static nint ScanFunction()
    {
        if (_addressScanned)
        {
            return _scanAddress;
        }
        _addressScanned = true;
        nint scanAddress = default;
        if (Svc.SigScanner.TryScanText("40 53 48 83 ec 20 e8 ?? ?? ?? ?? 45 33 c9 4c 8d 05 ?? ?? ?? ?? 48 8b d0 48 8b c8 48 8b d8 e8 ?? ?? ?? ?? 48 8b 8b ?? ?? ?? ?? 48 83 c4 20 5b e9 ?? ?? ?? ??", out scanAddress))
        {
            _scanAddress = scanAddress;
        }
        return _scanAddress;
    }

    private unsafe static int ResolveListOffset()
    {
        if (_listOffset != 0)
        {
            return _listOffset;
        }
        _listOffset = 5272;
        if (!_offsetResolved)
        {
            _offsetResolved = true;
            nint funcAddr = ScanFunction();
            if (!IsInvalidPointer(funcAddr))
            {
                try
                {
                    byte* ptr = (byte*)funcAddr;
                    if (ptr[14] == 76 && ptr[15] == 141 && ptr[16] == 5)
                    {
                        int relOffset = *(int*)(ptr + 17);
                        nint targetAddr = funcAddr + 21 + relOffset;
                        if (!IsInvalidPointer(targetAddr))
                        {
                            byte* dataPtr = (byte*)targetAddr;
                            for (int i = 0; i <= 280; i++)
                            {
                                if (dataPtr[i] == 73 && dataPtr[i + 1] == 139 && dataPtr[i + 2] == 133)
                                {
                                    _listOffset = *(int*)(dataPtr + i + 3);
                                    break;
                                }
                            }
                        }
                    }
                }
                catch
                {
                    _listOffset = 5272;
                }
            }
        }
        return _listOffset;
    }

    private static bool IsInvalidPointer(nint pointer)
    {
        long addr = pointer;
        if (addr >= 65536)
        {
            return addr > 140737488355327L;
        }
        return true;
    }

    public IEnumerator<VFXListData> GetEnumerator()
    {
        VFXListData[] entries = ListSpan.ToArray();
        for (int i = 0; i < entries.Length; i++)
        {
            VFXListData entry = entries[i];
            if (entry.IsValid())
            {
                yield return entry;
            }
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
