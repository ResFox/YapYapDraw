using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.ForkedTower;

public class Firestrike : ISpecialAction
{
    public override string Name => "Firestrike";

    public override HashSet<uint> ActionID => new HashSet<uint> { 42503u };

    public override uint Phase => 2u;

    public override void OnAbilityCast(ActorAbilityInfo info)
    {
        DrawManager.Draw(new DrawElement
        {
            drawAvfx = "general02pxf",
            Position = info.Source.Position,
            drawOnObject = false,
            radiusX = 5f,
            radiusZ = 70f,
            target = info.Target,
            hitCounter = new HitCounter
            {
                ActionID = new HashSet<uint> { 42502u }
            }
        });
    }
}
