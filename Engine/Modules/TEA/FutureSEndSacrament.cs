using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;
using YapYapDraw.Engine.Util;

namespace YapYapDraw.Modules.TEA;

public class FutureSEndSacrament : ISpecialAction
{
    public override string Name => "Future's End α (Sacrament)";

    public override uint Phase => 4u;

    public override HashSet<uint> ActionID => new HashSet<uint> { 18591u };

    public override void OnAbilityCast(ActorAbilityInfo info)
    {
        DrawElement obj = new DrawElement
        {
            drawAvfx = "general_x02f",
            radiusX = 8f,
            radiusZ = 100f,
            drawOnObject = true,
            hitCounter = new HitCounter
            {
                ActionID = new HashSet<uint> { 18569u },
                TargetHitCount = 3
            }
        };
        DrawManager.Draw(obj, info.Source);
        obj.refRotation = 90.Degrees();
        DrawManager.Draw(obj, info.Source);
    }
}
