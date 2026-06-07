using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.ClientState.Objects.Types;
using YapYapDraw.Engine.Interop;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.ModuleSetup;

namespace YapYapDraw.Modules.M8S;

public class AlphaWindStone : ISpecialAction
{
    public override string Name => "Alpha Stone/Wind";

    public override HashSet<uint> ActionID => new HashSet<uint>();

    public override void OnTargetIconEvent(IGameObject target, uint icon, ulong TargetID)
    {
        if (icon != 23 || ++base.NumCasts % 2 != 0)
        {
            return;
        }
        IGameObject addA = Svc.Objects.FirstOrDefault((IGameObject o) => o.BaseId == 18225);
        IGameObject addB = Svc.Objects.FirstOrDefault((IGameObject o) => o.BaseId == 18219);
        foreach (IGameObject tank in PlayerHelper.Tank)
        {
            IGameObject add = (tank.HasStatus(4389u) ? addA : addB);
            DrawManager.Draw(new DrawElement
            {
                drawAvfx = "gl_fan090_1bf",
                radiusX = 40f,
                radiusZ = 40f,
                drawOnObject = true,
                target = tank,
                hitCounter = new HitCounter
                {
                    ActionID = new HashSet<uint> { 41933u, 41954u }
                }
            }, add);
        }
    }
}
