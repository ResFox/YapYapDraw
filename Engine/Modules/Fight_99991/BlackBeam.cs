using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.Fight_99991;

public class BlackBeam : ISpecialAction
{
    public override string Name => "Black Beam";

    public override HashSet<uint> ActionID => new HashSet<uint> { 35567u };

    public override void OnAbilityCast(ActorAbilityInfo info)
    {
        DrawManager.Draw(new DrawElement
        {
            drawAvfx = "general02pxf",
            Position = info.Pos,
            drawOnObject = false,
            radiusX = 6f,
            radiusZ = 60f,
            target = info.Target,
            hitCounter = new HitCounter
            {
                ActionID = new HashSet<uint> { 35570u },
                TargetHitCount = 5
            }
        });
    }
}
