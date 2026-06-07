using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace YapYapDraw.Engine.Properties;

internal static class Resources
{
    private static readonly Dictionary<string, byte[]> Cache = new();

    private static byte[] Load(string name)
    {
        if (Cache.TryGetValue(name, out var cached))
            return cached;

        var asm = typeof(Resources).Assembly;
        var suffix = "Resources." + name + ".bin";
        string? resName = null;
        foreach (var n in asm.GetManifestResourceNames())
        {
            if (n.EndsWith(suffix, StringComparison.Ordinal)) { resName = n; break; }
        }
        if (resName == null)
            throw new InvalidOperationException("Missing embedded resource: " + name);

        using var stream = asm.GetManifestResourceStream(resName)
            ?? throw new InvalidOperationException("Missing embedded resource stream: " + name);
        using var ms = new MemoryStream();
        stream.CopyTo(ms);
        var bytes = ms.ToArray();
        Cache[name] = bytes;
        return bytes;
    }

    internal static byte[] eyewarn => Load("eyewarn");
    internal static byte[] Share2_6m_5s_omen => Load("Share2_6m_5s_omen");
    internal static byte[] ShareLazer5sGround => Load("ShareLazer5sGround");
    internal static byte[] tank_lockon_3m_5s_noc => Load("tank_lockon_3m_5s_noc");
    internal static byte[] tank_lockon_5m_5s_noc => Load("tank_lockon_5m_5s_noc");
    internal static byte[] tank_lockon_8s_noc => Load("tank_lockon_8s_noc");
    internal static byte[] TankFan90 => Load("TankFan90");
    internal static byte[] tmp_circle => Load("tmp_circle");
    internal static byte[] tmp_donut => Load("tmp_donut");
    internal static byte[] tmp_fan => Load("tmp_fan");
    internal static byte[] tmp_org_donut => Load("tmp_org_donut");
    internal static byte[] tmp_org_fan => Load("tmp_org_fan");
    internal static byte[] tmp_rect => Load("tmp_rect");
    internal static byte[] tmp_rect2 => Load("tmp_rect2");
}
