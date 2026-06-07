using Dalamud.Game.ClientState.Objects.Types;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using YapYapDraw.Engine.Enum;

namespace YapYapDraw.Engine.Util;

public static class HeaderMarker
{
    public static unsafe HeaderMarkerEnum Mark(this ulong objectId)
    {
        foreach (HeaderMarkerEnum marker in System.Enum.GetValues<HeaderMarkerEnum>())
        {
            if (marker == HeaderMarkerEnum.None) continue;
            var marked = ((MarkingController*)MarkingController.Instance())->Markers[(int)marker];
            if (objectId == marked) return marker;
        }
        return HeaderMarkerEnum.None;
    }
}
