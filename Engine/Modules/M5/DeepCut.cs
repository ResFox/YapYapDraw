using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.ClientState.Objects.Types;
using YapYapDraw.Engine.Interop;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Util;

namespace YapYapDraw.Modules.M5;

public class DeepCut : ISpecialAction
{
    public override string Name => "Deep Cut";

    public override HashSet<uint> ActionID => new HashSet<uint>();

    public override uint WeatherID => 2u;

    public override void OnTargetIconEvent(IGameObject target, uint icon, ulong TargetID)
    {
        IGameObject source = Svc.Objects.FirstOrDefault((IGameObject o) => o.BaseId == 18358);
        if (icon == 471 && source != null)
        {
            HitCounter hitCounter = new HitCounter
            {
                ActionID = new HashSet<uint> { 42694u }
            };
            SimpleElement.FanToTarget(source, target, 60f, 45, Follow: true, default, 0f, 3000f, hitCounter);
        }
    }
}
