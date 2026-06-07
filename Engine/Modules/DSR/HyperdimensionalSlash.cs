using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.ClientState.Objects.Types;
using YapYapDraw.Engine.Interop;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.ModuleSetup;

namespace YapYapDraw.Modules.DSR;

public class HyperdimensionalSlash : ISpecialAction
{
    public override string Name => "Hyperdimensional Slash";

    public override uint Phase => 1u;

    public override HashSet<uint> ActionID => new HashSet<uint>();

    public override void OnTargetIconEvent(IGameObject target, uint icon, ulong TargetID)
    {
        if (icon == 234)
        {
            IGameObject target2 = Svc.Objects.FirstOrDefault((IGameObject obj) => obj.BaseId == 12602);
            DrawManager.Draw(new DrawElement
            {
                drawAvfx = "general02xf",
                radiusX = 4f,
                radiusZ = 70f,
                drawOnObject = true,
                target = target,
                hitCounter = new HitCounter
                {
                    ActionID = new HashSet<uint> { 25303u }
                }
            }, target2);
        }
    }
}
