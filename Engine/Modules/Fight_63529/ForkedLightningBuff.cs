using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using YapYapDraw.Engine.Interop;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Enum;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.Fight_63529;

public class ForkedLightningBuff : ISpecialAction
{
    public override string Name => "Forked Lightning (buff)";

    public override uint Phase => 1u;

    public override HashSet<uint> ActionID => new HashSet<uint>();

    public override void OnAddStatus(ActorStatusChangeInfo info)
    {
        if (info.StatusID == 3799)
        {
            IGameObject target = Svc.Objects.SearchById(info.TargetID);
            if (target != null)
            {
                DrawManager.Draw(new DrawElement
                {
                    drawType = ElementType.Omen,
                    drawAvfx = "general_1bxf",
                    radiusX = 5f,
                    radiusZ = 5f,
                    drawOnObject = true,
                    delayDrawTime = 71000f,
                    destroyTime = 5000f
                }, target);
            }
        }
    }
}
