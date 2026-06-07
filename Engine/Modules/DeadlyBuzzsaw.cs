using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.ClientState.Objects.Types;
using YapYapDraw.Engine.Interop;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.ModuleSetup;

namespace YapYapDraw.Modules.M9S;

public class DeadlyBuzzsaw : ISpecialAction
{
    public override string Name => "Deadly Buzzsaw";

    public override HashSet<uint> ActionID => new HashSet<uint>();

    public override void OnEnvControl(byte index, uint state)
    {
        if (index != 0 || state != 131073)
        {
            return;
        }
        foreach (IGameObject item in Svc.Objects.Where(x => x.BaseId - 19189 <= 1))
        {
            DrawManager.Draw(new DrawElement
            {
                drawAvfx = "general02xf",
                drawOnObject = true,
                radiusX = 2.5f,
                radiusZ = ((item.BaseId == 19189) ? 10 : 20),
                hitCounter = new HitCounter
                {
                    ActionID = new HashSet<uint> { 45926u },
                    TargetHitCount = 2
                }
            }, item);
        }
    }
}
