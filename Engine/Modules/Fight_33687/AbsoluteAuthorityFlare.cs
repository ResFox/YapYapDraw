using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.ModuleSetup;

namespace YapYapDraw.Modules.Fight_33687;

public class AbsoluteAuthorityFlare : ISpecialAction
{
    public override string Name => "Absolute Authority (flare)";

    public override HashSet<uint> ActionID => new HashSet<uint>();

    public override void OnTargetIconEvent(IGameObject target, uint icon, ulong TargetID)
    {
        if (icon == 327)
        {
            SimpleElement.Circle(target, 12f, 3000f, 0f, new HitCounter
            {
                ActionID = new HashSet<uint> { 39518u }
            });
        }
    }
}
