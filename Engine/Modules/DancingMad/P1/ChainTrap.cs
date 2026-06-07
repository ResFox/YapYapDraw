using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.DancingMad.P1;

public class ChainTrap : ISpecialAction
{
    public override string Name => "Chain Trap";

    public override HashSet<uint> ActionID => new HashSet<uint>();

    public override void OnAddStatus(ActorStatusChangeInfo info)
    {
        if (info.StatusID != 5078)
            return;

        IGameObject? target = info.TargetID.GameObject();
        if (target == null)
            return;

        DrawManager.Draw(new DrawElement
        {
            drawAvfx = "nockback_omen04t1",
            radiusX = 6f,
            radiusZ = 6f,
            delayDrawTime = (info.Time - 5f) * 1000f,
            StatusCheck = new StatusCheck
            {
                CheckObject = target,
                Status = 5078u
            },
            hitCounter = new HitCounter
            {
                ActionID = new HashSet<uint> { 47783u },
                HitTarget = target
            }
        }, target);
    }
}
