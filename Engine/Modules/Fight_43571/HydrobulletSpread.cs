using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;
using YapYapDraw.Engine.Util;

namespace YapYapDraw.Modules.Fight_43571;

public class HydrobulletSpread : ISpecialAction
{
    public override string Name => "Hydrobullet (spread)";

    public override uint Phase => 1u;

    public override HashSet<uint> ActionID => new HashSet<uint>();

    public override void OnAddStatus(ActorStatusChangeInfo info)
    {
        if (info.StatusID == 3748)
        {
            IGameObject target = info.TargetID.GameObject();
            if (target != null)
            {
                DrawManager.Draw(new DrawElement
                {
                    drawAvfx = "general_1bxf",
                    radiusX = 15f,
                    radiusZ = 15f,
                    drawOnObject = true,
                    destroyTime = 8000f,
                    delayDrawTime = (int)((info.Time - 7f) * 1000f)
                }, target);
            }
        }
    }
}
