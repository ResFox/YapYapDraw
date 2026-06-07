using System.Security.Cryptography;
using System.Text;

namespace YapYapDraw.Engine.Host;

internal static class ModuleLoader
{
    public static string Sha256Hex(string input)
    {
        byte[] hash = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        StringBuilder sb = new();
        for (int i = 0; i < hash.Length; i++)
        {
            sb.Append(hash[i].ToString("x2"));
        }
        return sb.ToString();
    }
}
