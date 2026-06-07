using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Text.Json;

namespace YapYapDraw.QuickDraws;

public static class ShareCodec
{
    public const string ModulePrefix = "YAPDRAWPACK1:";
    public const string DrawPrefix   = "YAPDRAW1:";

    private static readonly JsonSerializerOptions Opts = new()
    {
        IncludeFields = true,
        WriteIndented = false,
    };

    public static string Encode<T>(string prefix, T value)
    {
        var json  = JsonSerializer.Serialize(value, Opts);
        var bytes = Encoding.UTF8.GetBytes(json);

        using var outMs = new MemoryStream();
        using (var gz = new GZipStream(outMs, CompressionLevel.Optimal, true))
            gz.Write(bytes, 0, bytes.Length);
        return prefix + Convert.ToBase64String(outMs.ToArray());
    }

    public static bool TryDecode<T>(string prefix, string code, out T? value)
    {
        value = default;
        if (string.IsNullOrWhiteSpace(code)) return false;
        code = code.Trim();
        if (code.StartsWith(prefix, StringComparison.Ordinal)) code = code[prefix.Length..];

        try
        {
            var packed = Convert.FromBase64String(code);
            using var inMs  = new MemoryStream(packed);
            using var gz    = new GZipStream(inMs, CompressionMode.Decompress);
            using var outMs = new MemoryStream();
            gz.CopyTo(outMs);
            var json = Encoding.UTF8.GetString(outMs.ToArray());
            value = JsonSerializer.Deserialize<T>(json, Opts);
            return value != null;
        }
        catch { return false; }
    }
}
