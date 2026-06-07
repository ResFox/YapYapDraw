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

namespace YapYapDraw.Modules.TEA;

public class OrdainedCapitalPunishment : ISpecialAction
{
    public override string Name => "Ordained Capital Punishment";

    public override uint Phase => 4u;

    public override HashSet<uint> ActionID => new HashSet<uint> { 18578u };

    public override void OnActionCast(ActorCastInfo info)
    {
        DrawManager.Draw(new DrawElement
        {
            drawAvfx = ((((ICharacter)Svc.Objects.LocalPlayer).GetRole() == CombatRole.Tank) ? "general_1bpxf" : "general_1bxf"),
            radiusX = 4f,
            radiusZ = 4f,
            alwaysDrawOnCurrentTarget = true,
            hitCounter = new HitCounter
            {
                ActionID = new HashSet<uint> { 18579u },
                TargetHitCount = 3
            }
        }, info.SourceId.GameObject());
    }
}
