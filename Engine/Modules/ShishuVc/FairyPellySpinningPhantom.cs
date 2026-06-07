using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Util;

namespace YapYapDraw.Modules.ShishuVc;

public class FairyPellySpinningPhantom : ISpecialAction
{
    public override string Name => "Fairy Pelly Spinning Phantom";

    public override uint Phase => 3u;

    public override HashSet<uint> ActionID => new HashSet<uint>();

    public override void OnTargetIconEvent(IGameObject Source, uint icon, ulong TargetID)
    {
        switch (icon)
        {
        case 647u:
            SimpleElement.Fan(Source, 40f, 180, Source.Rotation.Radians(), 3000f, 0f, new HitCounter
            {
                ActionID = new HashSet<uint> { 45469u, 45670u }
            });
            break;
        case 646u:
            SimpleElement.Fan(Source, 40f, 180, Source.Rotation.Radians() + 180.Degrees(), 3000f, 0f, new HitCounter
            {
                ActionID = new HashSet<uint> { 45469u, 45670u }
            });
            break;
        }
    }
}
