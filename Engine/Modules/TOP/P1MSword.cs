using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.ClientState.Objects.Types;
using YapYapDraw.Engine.Interop;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.TOP;

public class P1MSword : ISpecialAction
{
    public override string Name => "P1 M-Sword";

    public override uint Phase => 2u;

    public override uint WeatherID => 78u;

    public override HashSet<uint> ActionID => new HashSet<uint> { 31550u };

    public override void OnAbilityCast(ActorAbilityInfo info)
    {
        base.CanDraw = true;
    }

    public override void OnTargetIconEvent(IGameObject target, uint icon, ulong TargetID)
    {
        if (icon != 100 || !base.CanDraw)
        {
            return;
        }
        base.CanDraw = false;
        foreach (IGameObject actor in Svc.Objects.Where(o =>
        {
            uint baseId = o.BaseId;
            return baseId - 15713 <= 1;
        }))
        {
            SimpleElement.Circle(actor, 10f, 3000f, 0f, new HitCounter
            {
                ActionID = new HashSet<uint> { 31526u }
            });
        }
    }
}
