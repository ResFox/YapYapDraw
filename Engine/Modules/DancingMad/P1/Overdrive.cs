using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.ClientState.Objects.Types;
using YapYapDraw.Engine.Interop;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.DancingMad.P1;

public class Overdrive : ISpecialAction
{
    public override string Name => "Overdrive";

    public override HashSet<uint> ActionID => new HashSet<uint> { 50722u };

    public override void OnAbilityCast(ActorAbilityInfo info)
    {
        DrawManager.Draw(new DrawElement
        {
            drawAvfx = "general_1bxf",
            radiusX = 5f,
            radiusZ = 5f,
            alwaysDrawOnCurrentTarget = true,
            hitCounter = new HitCounter
            {
                ActionID = new HashSet<uint> { 49739u },
                TargetHitCount = 3
            }
        }, Svc.Objects.FirstOrDefault((IGameObject o) => o.BaseId == 19504));
    }
}
