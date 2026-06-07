using FFXIVClientStructs.FFXIV.Client.Game.Event;

namespace YapYapDraw.Engine.Util;

public static class MapUtil
{
    public unsafe static nint GetMapEffectModule()
        => *(nint*)((byte*)EventFramework.Instance() + 344);
}
