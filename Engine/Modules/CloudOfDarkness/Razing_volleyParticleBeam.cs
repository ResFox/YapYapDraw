using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;
using YapYapDraw.Engine.Util;

namespace YapYapDraw.Modules.CloudOfDarkness;

public class Razing_volleyParticleBeam : ISpecialAction
{
    public override string Name => "Razing-volley Particle Beam";

    public override HashSet<uint> ActionID => new HashSet<uint> { 40511u };

    public override uint Phase => 2u;

    public override void OnActionCast(ActorCastInfo info)
    {
        DrawElement element = new DrawElement
        {
            drawAvfx = "general02xf",
            radiusX = 4f,
            radiusZ = 45f,
            drawOnObject = true,
            destroyTime = 4000f,
            delayDrawTime = 4000f
        };
        aoes.Add(DrawManager.Draw(element, info.SourceId.GameObject()));
    }

    public override void OnAbilityCast(ActorAbilityInfo info)
    {
        if (aoes.Count > 0)
        {
            aoes.RemoveAt(0);
        }
    }
}
