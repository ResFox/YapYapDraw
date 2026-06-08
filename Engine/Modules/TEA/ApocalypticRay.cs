using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.TEA;

public class ApocalypticRay : ISpecialAction
{
    public override string Name => "Apocalyptic Ray";

    public override uint Phase => 2u;

    public override HashSet<uint> ActionID => new HashSet<uint> { 18507u };

    public override void OnAbilityCast(ActorAbilityInfo info)
    {
        DrawManager.Draw(new DrawElement
        {
            drawAvfx = "gl_fan090_1bf",
            radiusX = 25f,
            radiusZ = 25f,
            drawOnObject = true,
            hitCounter = new HitCounter
            {
                ActionID = new HashSet<uint> { 18508u },
                TargetHitCount = 5
            }
        }, info.Source);
    }
}
