using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.ClientState.Objects.Types;
using YapYapDraw.Engine.Interop;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Util;

namespace YapYapDraw.Modules.M2S;

public class StingingSlash : ISpecialAction
{
    public override string Name => "Stinging Slash";

    public override uint Phase => 2u;

    public override HashSet<uint> ActionID => new HashSet<uint>();

    public override void OnTargetIconEvent(IGameObject target, uint icon, ulong TargetID)
    {
        if (icon == 471)
        {
            IGameObject? source = Svc.Objects.FirstOrDefault((IGameObject x) => x.BaseId == 16941);
            HitCounter hitCounter = new HitCounter
            {
                ActionID = new HashSet<uint> { 37277u }
            };
            SimpleElement.FanToTarget(source, target, 50f, 90, Follow: true, default, 0f, 3000f, hitCounter);
        }
    }
}
