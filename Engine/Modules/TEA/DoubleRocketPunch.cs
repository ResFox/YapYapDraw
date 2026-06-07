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

public class DoubleRocketPunch : ISpecialAction
{
    public override string Name => "Double Rocket Punch";

    public override uint Phase => 2u;

    public override HashSet<uint> ActionID => new HashSet<uint> { 18503u };

    public override void OnActionCast(ActorCastInfo info)
    {
        DrawElement drawElement = new DrawElement
        {
            drawAvfx = "general_1bxf",
            radiusX = 3f,
            radiusZ = 3f,
            hitCounter = new HitCounter
            {
                ActionID = new HashSet<uint> { 18503u }
            }
        };
        if (((ICharacter)Svc.Objects.LocalPlayer).GetRole() == CombatRole.Tank)
        {
            drawElement.drawAvfx = "general_1bpxf";
        }
        DrawManager.Draw(drawElement, info.TargetId.GameObject());
    }
}
