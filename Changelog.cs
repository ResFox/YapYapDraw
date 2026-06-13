using System.Reflection;

namespace YapYapDraw;

public static class Changelog
{
    public static readonly string Version =
        Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "1.0.0.0";

    public const string Title = "What's new";

    public static readonly string[] Notes =
    {
        "Improved wipe logic.",
    };
}
