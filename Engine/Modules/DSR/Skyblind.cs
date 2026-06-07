using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using YapYapDraw.Engine.Interop;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.DSR;

public class Skyblind : ISpecialAction
{
    public override string Name => "Skyblind";

    public override uint Phase => 1u;

    public override HashSet<uint> ActionID => new HashSet<uint>();

    public override void OnRemoveStatus(ActorStatusChangeInfo info)
    {
        if (info.StatusID == 2661)
        {
            IGameObject target = Svc.Objects.SearchById(info.TargetID);
            if (target != null)
            {
                DrawManager.Draw(new DrawElement
                {
                    drawAvfx = "general_1bxf",
                    Position = target.Position,
                    drawOnObject = false,
                    radiusX = 3f,
                    radiusZ = 3f,
                    destroyTime = 2500f
                }, target);
            }
        }
    }
}
