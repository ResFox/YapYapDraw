using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.ClientState.Objects.Types;
using YapYapDraw.Engine.Interop;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.TOP;

public class SolarRay : ISpecialAction
{
    public override string Name => "Solar Ray";

    public override uint Phase => 5u;

    public override HashSet<uint> ActionID => new HashSet<uint> { 33196u, 33197u };

    public override void OnAbilityCast(ActorAbilityInfo info)
    {
        IGameObject target = Svc.Objects.FirstOrDefault((IGameObject o) => o.BaseId == 15720);
        DrawManager.Draw(new DrawElement
        {
            drawAvfx = "general_1bxf",
            radiusX = 5f,
            radiusZ = 5f,
            drawOnObject = true,
            alwaysDrawOnCurrentTarget = true,
            hitCounter = new HitCounter
            {
                ActionID = new HashSet<uint> { 31489u, 31490u }
            }
        }, target);
    }
}
