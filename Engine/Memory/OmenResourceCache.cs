using System;
using System.Collections.Generic;
using System.IO.Hashing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using Dalamud.Utility;
using YapYapDraw.Engine.Interop;
using YapYapDraw.Engine.Vfx;
using YapYapDraw.Engine;
using YapYapDraw.Engine.Properties;

namespace YapYapDraw.Engine.Memory;

internal static class OmenResourceCache
{
    private static readonly Mutex ResourceLock = new Mutex();

    private static string _circlePath = string.Empty;

    private static string _rectPath = string.Empty;

    private static string _rect2Path = string.Empty;

    private static readonly HashSet<float> _cachedFans = new HashSet<float>();

    private static readonly HashSet<float> _cachedDonuts = new HashSet<float>();

    private static readonly HashSet<string> _cachedPaths = new HashSet<string>();

    public unsafe static void AddResource(string path, byte[] data)
    {
        Plugin.DebugLog("ResourceAdd: " + path);
        ResourceLock.WaitOne();
        try
        {
            uint category = 8u;
            uint type = 1635149432u;
            Crc32 crc = new Crc32();
            byte[] pathBytes = Encoding.UTF8.GetBytes(path);
            crc.Append(pathBytes);
            uint hash = BitConverter.ToUInt32(crc.GetCurrentHash());
            if (ClientOmenHooks.ResourceManagerAddress == 0)
            {
                Svc.Log.Warning("ResourceManagerAddress is 0x0, skipping resource add.");
                return;
            }
            nint pathPtr = Marshal.StringToHGlobalAnsi(path);
            try
            {
                nint handle = ClientOmenHooks.getResource(ClientOmenHooks.ResourceManagerAddress, (nint)(&category), (nint)(&type), (nint)(&hash), pathPtr, IntPtr.Zero);
                if (handle == IntPtr.Zero)
                {
                    Svc.Log.Warning("Failed to get resource.");
                    return;
                }
                Marshal.WriteByte(handle + 168, 2);
                Marshal.WriteByte(handle + 169, 7);
                void* filePtr = ((IntPtr)IntPtr.Add(handle, 192)).ToPointer();
                nint buffer = Marshal.AllocHGlobal(data.Length);
                try
                {
                    Marshal.Copy(data, 0, buffer, data.Length);
                    ClientOmenHooks.loadResource(Marshal.ReadIntPtr((nint)filePtr), buffer, (uint)data.Length, handle);
                    ClientOmenHooks.finalizeResource(handle);
                }
                finally
                {
                    Marshal.FreeHGlobal(buffer);
                }
            }
            finally
            {
                Marshal.FreeHGlobal(pathPtr);
            }
        }
        catch (Exception ex)
        {
            Svc.Log.Error("Error in ResourceAdd: " + ex);
        }
        finally
        {
            ResourceLock.ReleaseMutex();
        }
    }

    public static void GetFan(float radian, out string path)
    {
        path = "vfx/omen/eff/yd/customFan" + $"{radian}".Replace(".", "_") + ".avfx";
        if (!_cachedFans.Contains(radian))
        {
            byte[] data = BuildFan(Resources.tmp_fan, radian);
            AddResource(path, data);
            _cachedFans.Add(radian);
        }
    }

    public static void GetDonut(float radius, out string path, float? angle = null)
    {
        path = "vfx/omen/eff/yd/customDonut" + $"{radius}".Replace(".", "_") + ".avfx";
        if (!_cachedDonuts.Contains(radius))
        {
            byte[] data = BuildDonut(Resources.tmp_donut, radius, angle);
            AddResource(path, data);
            _cachedDonuts.Add(radius);
        }
    }

    public static void GetCircle(out string path)
    {
        path = "vfx/omen/eff/yd/customCircle.avfx";
        if (StringExtensions.IsNullOrEmpty(_circlePath))
        {
            byte[] tmp_circle = Resources.tmp_circle;
            AddResource(path, tmp_circle);
            _circlePath = path;
        }
    }

    public static void GetRect(out string path)
    {
        path = "vfx/omen/eff/yd/customRect.avfx";
        if (StringExtensions.IsNullOrEmpty(_rectPath))
        {
            byte[] tmp_rect = Resources.tmp_rect;
            AddResource(path, tmp_rect);
            _rectPath = path;
        }
    }

    public static void GetRect2(out string path)
    {
        path = "vfx/omen/eff/yd/customRect2.avfx";
        if (StringExtensions.IsNullOrEmpty(_rect2Path))
        {
            byte[] tmp_rect = Resources.tmp_rect2;
            AddResource(path, tmp_rect);
            _rect2Path = path;
        }
    }

    public static void RegisterRaw(byte[] data, string path)
    {
        if (!_cachedPaths.Contains(path))
        {
            AddResource(path, data);
            _cachedPaths.Add(path);
        }
    }

    public static byte[] BuildFan(byte[] template, float radian)
    {
        byte[] curve = BitConverter.GetBytes((float)((1.0 - Math.Cos(radian / 2f)) / 2.0));
        byte[] offset = BitConverter.GetBytes(0.45333326f - 10f / (float)Math.PI * radian);
        byte[] width = BitConverter.GetBytes(5.407703f + 14.222406f * radian);
        byte[] result = template.ToArray();
        Buffer.BlockCopy(curve, 0, result, 6076, curve.Length);
        Buffer.BlockCopy(offset, 0, result, 6800, offset.Length);
        Buffer.BlockCopy(offset, 0, result, 7284, offset.Length);
        Buffer.BlockCopy(curve, 0, result, 9588, curve.Length);
        Buffer.BlockCopy(width, 0, result, 10312, width.Length);
        Buffer.BlockCopy(width, 0, result, 10796, width.Length);
        Buffer.BlockCopy(curve, 0, result, 13100, curve.Length);
        return result;
    }

    public static byte[] BuildDonut(byte[] template, float radius, float? angle = null)
    {
        byte[] arc = BitConverter.GetBytes(angle.HasValue ? ((float)((1.0 - Math.Cos(angle.Value / 2f)) / 2.0)) : 1f);
        float inner = 0.5f * (1f - radius) / (1f + radius);
        byte[] innerBytes = BitConverter.GetBytes(inner);
        byte[] scaleBytes = BitConverter.GetBytes(1f / (0.5f + inner));
        byte[] result = new byte[template.Length];
        template.CopyTo(result, 0);
        Buffer.BlockCopy(scaleBytes, 0, result, 388, scaleBytes.Length);
        Buffer.BlockCopy(scaleBytes, 0, result, 412, scaleBytes.Length);
        Buffer.BlockCopy(arc, 0, result, 6044, arc.Length);
        Buffer.BlockCopy(innerBytes, 0, result, 6088, innerBytes.Length);
        Buffer.BlockCopy(arc, 0, result, 8772, arc.Length);
        Buffer.BlockCopy(innerBytes, 0, result, 8816, innerBytes.Length);
        Buffer.BlockCopy(arc, 0, result, 11500, arc.Length);
        Buffer.BlockCopy(innerBytes, 0, result, 11544, innerBytes.Length);
        return result;
    }
}
