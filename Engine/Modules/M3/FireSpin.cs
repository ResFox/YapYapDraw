using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;
using YapYapDraw.Engine.Util;

namespace YapYapDraw.Modules.M3;

public class FireSpin : ISpecialAction
{
    private bool Clockwise = true;

    public override string Name => "Fire Spin";

    public override uint Phase => 1u;

    public override HashSet<uint> ActionID => new HashSet<uint> { 39768u };

    public override void OnActionCast(ActorCastInfo info)
    {
        for (int i = 1; i <= 7; i++)
        {
            SimpleElement.Fan(info.SourceId, 40f, 60, info.Facing + (Clockwise ? (-(45.Degrees() * i)) : (45.Degrees() * i)), 5000f, 1000 * i, 0u);
        }
    }

    public override void OnTargetIconEvent(IGameObject target, uint icon, ulong TargetID)
    {
        if (target.BaseId == 17090)
        {
            if (icon == 167)
            {
                Clockwise = true;
            }
            else
            {
                Clockwise = false;
            }
        }
    }
}
