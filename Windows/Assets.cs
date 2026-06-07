using System;
using System.Linq;
using System.Reflection;
using Dalamud.Interface.Textures.TextureWraps;

namespace YapYapDraw.Windows;

public static class Assets
{
    private static IDalamudTextureWrap? _logo;
    private static bool _started;

    public static IDalamudTextureWrap? Logo
    {
        get
        {
            if (!_started) { _started = true; LoadLogo(); }
            return _logo;
        }
    }

    private static async void LoadLogo()
    {
        try
        {
            var asm  = Assembly.GetExecutingAssembly();
            var name = asm.GetManifestResourceNames()
                .FirstOrDefault(n => n.EndsWith("yapyap_logo.png", StringComparison.OrdinalIgnoreCase));
            if (name == null) return;

            using var s = asm.GetManifestResourceStream(name)!;
            var buf = new byte[s.Length];
            s.ReadExactly(buf);
            _logo = await Plugin.TextureProvider.CreateFromImageAsync(buf);
        }
        catch { }
    }

    public static void Dispose()
    {
        _logo?.Dispose();
        _logo = null;
        _started = false;
    }
}
