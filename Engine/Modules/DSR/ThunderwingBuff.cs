using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using YapYapDraw.Engine.Interop;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.DSR;

public class ThunderwingBuff : ISpecialAction
{
    public override string Name => "Thunderwing (buff)";

    public override uint Phase => 5u;

    public override HashSet<uint> ActionID => new HashSet<uint>();

    public override void OnAddStatus(ActorStatusChangeInfo info)
    {
        if (info.StatusID == 2833)
        {
            IGameObject target = Svc.Objects.SearchById(info.TargetID);
            if (target != null)
            {
                DrawManager.Draw(new DrawElement
                {
                    drawAvfx = "general_1bxf",
                    radiusX = 5f,
                    radiusZ = 5f,
                    drawOnObject = true,
                    delayDrawTime = (int)(info.Time - 5f) * 1000,
                    hitCounter = new HitCounter
                    {
                        ActionID = new HashSet<uint> { 27536u }
                    }
                }, target);
            }
        }
    }
}
