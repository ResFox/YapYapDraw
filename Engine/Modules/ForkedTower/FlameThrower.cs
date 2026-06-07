using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.ForkedTower;

public class FlameThrower : ISpecialAction
{
    public override string Name => "Flame Thrower";

    public override HashSet<uint> ActionID => new HashSet<uint> { 42444u };

    public override uint Phase => 2u;

    public override void OnAbilityCast(ActorAbilityInfo info)
    {
        DrawManager.Draw(new DrawElement
        {
            drawAvfx = "general02pxf",
            Position = info.Source.Position,
            drawOnObject = false,
            radiusX = 4f,
            radiusZ = 40f,
            target = info.Target,
            hitCounter = new HitCounter
            {
                ActionID = new HashSet<uint> { 42445u }
            }
        });
    }
}
