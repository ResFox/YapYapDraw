using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Enum;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;
using YapYapDraw.Engine.Util;

namespace YapYapDraw.Modules.Fight_43571;

public class ExplosionTarget : ISpecialAction
{
    public override string Name => "Explosion (target)";

    public override uint Phase => 2u;

    public override HashSet<uint> ActionID => new HashSet<uint>();

    public override void OnAddStatus(ActorStatusChangeInfo info)
    {
        if (info.StatusID == 3723)
        {
            IGameObject target = info.TargetID.GameObject();
            if (target != null)
            {
                DrawManager.Draw(new DrawElement
                {
                    drawType = ElementType.LockOn,
                    drawAvfx = "m0796_trg_shar_0a1",
                    drawOnObject = true,
                    delayDrawTime = (int)((info.Time - 14f) * 1000f)
                }, target);
            }
        }
    }
}
