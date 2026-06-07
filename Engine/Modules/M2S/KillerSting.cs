using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using YapYapDraw.Engine.Interop;
using YapYapDraw.Engine.Interop.Game;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.ModuleSetup;

namespace YapYapDraw.Modules.M2S;

public class KillerSting : ISpecialAction
{
    public override string Name => "Killer Sting";

    public override uint Phase => 1u;

    public override HashSet<uint> ActionID => new HashSet<uint>();

    public override void OnTargetIconEvent(IGameObject target, uint icon, ulong TargetID)
    {
        if (icon == 259)
        {
            DrawElement drawElement = new DrawElement
            {
                drawAvfx = "general_1bxf",
                radiusX = 6f,
                radiusZ = 6f,
                drawOnObject = true,
                hitCounter = new HitCounter
                {
                    ActionID = new HashSet<uint> { 37278u }
                }
            };
            if (((ICharacter)Svc.Objects.LocalPlayer).GetRole() == CombatRole.Tank)
            {
                drawElement.drawAvfx = "general_1bpxf";
            }
            DrawManager.Draw(drawElement, target);
        }
    }
}
