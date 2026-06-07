using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using YapYapDraw.Engine.Interop;
using YapYapDraw.Engine.Interop.Game;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;
using YapYapDraw.Engine.Util;

namespace YapYapDraw.Modules.M3S;

public class KnuckleSandwich : ISpecialAction
{
    public override string Name => "Knuckle Sandwich";

    public override uint Phase => 1u;

    public override HashSet<uint> ActionID => new HashSet<uint> { 37923u };

    public override void OnActionCast(ActorCastInfo info)
    {
        StatusHelper.GetStack(info.SourceId, 4022u, out var stack);
        DrawElement drawElement = new DrawElement
        {
            drawAvfx = "general_1bxf",
            radiusX = 6f,
            radiusZ = 6f,
            drawOnObject = true,
            alwaysDrawOnCurrentTarget = true,
            hitCounter = new HitCounter
            {
                ActionID = new HashSet<uint> { 37923u, 37924u },
                TargetHitCount = 4 + stack * 2
            }
        };
        if (((ICharacter)Svc.Objects.LocalPlayer).GetRole() == CombatRole.Tank)
        {
            drawElement.drawAvfx = "general_1bpxf";
        }
        DrawManager.Draw(drawElement, info.SourceId.GameObject());
    }
}
