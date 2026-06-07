using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Enum;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;
using YapYapDraw.Engine.Util;

namespace YapYapDraw.Modules.Fight_43571;

public class HydrobulletStack : ISpecialAction
{
    public override string Name => "Hydrobullet (stack)";

    public override uint Phase => 1u;

    public override HashSet<uint> ActionID => new HashSet<uint>();

    public override void OnAddStatus(ActorStatusChangeInfo info)
    {
        if (info.StatusID == 3747)
        {
            IGameObject target = info.TargetID.GameObject();
            if (target != null)
            {
                DrawManager.Draw(new DrawElement
                {
                    drawType = ElementType.LockOn,
                    drawAvfx = "com_share0c",
                    drawOnObject = true,
                    delayDrawTime = (int)((info.Time - 5f) * 1000f)
                }, target);
            }
        }
    }
}
