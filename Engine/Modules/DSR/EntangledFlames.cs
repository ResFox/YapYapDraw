using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using YapYapDraw.Engine.Interop;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Enum;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.DSR;

public class EntangledFlames : ISpecialAction
{
    public override string Name => "Entangled Flames (buff)";

    public override uint Phase => 6u;

    public override HashSet<uint> ActionID => new HashSet<uint>();

    public override void OnAddStatus(ActorStatusChangeInfo info)
    {
        if (info.StatusID == 2759)
        {
            IGameObject target = Svc.Objects.SearchById(info.TargetID);
            if (target != null)
            {
                DrawManager.Draw(new DrawElement
                {
                    drawAvfx = "2tagup_3m_6s_x",
                    drawType = ElementType.LockOn,
                    drawOnObject = true,
                    delayDrawTime = (int)(info.Time - 8f) * 1000
                }, target);
            }
        }
    }
}
