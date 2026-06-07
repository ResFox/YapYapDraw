using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;
using YapYapDraw.Engine.Util;

namespace YapYapDraw.Modules.TOP;

public class P5FarWorldBuff : ISpecialAction
{
    public override string Name => "Far World (buff)";

    public override uint Phase => 5u;

    public override HashSet<uint> ActionID => new HashSet<uint>();

    public override void OnAddStatus(ActorStatusChangeInfo info)
    {
        if (info.StatusID == 3443)
        {
            IGameObject target = info.TargetID.GameObject();
            if (target != null)
            {
                DrawManager.Draw(new DrawElement
                {
                    drawAvfx = "general_1bxf",
                    radiusX = 8f,
                    radiusZ = 8f,
                    drawOnObject = true,
                    delayDrawTime = (int)(info.Time - 6f) * 1000,
                    hitCounter = new HitCounter
                    {
                        ActionID = new HashSet<uint> { 33040u },
                        HitTarget = target
                    }
                }, target);
            }
        }
    }
}
