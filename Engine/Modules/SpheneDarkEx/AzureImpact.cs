using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.SpheneDarkEx;
public class AzureImpact : ISpecialAction
{
    public override string Name => "Azure Impact";

    public override HashSet<uint> ActionID => new HashSet<uint> { 44592u };

    public override void OnAbilityCast(ActorAbilityInfo info)
    {
        DrawManager.Draw(new DrawElement
        {
            drawAvfx = "er_gl_fan100_o0v",
            radiusX = 100f,
            radiusZ = 100f,
            alwaysFaceCurrentTarget = true,
            hitCounter = new HitCounter
            {
                ActionID = new HashSet<uint> { 44593u },
                TargetHitCount = 2
            }
        }, info.Source);
    }
}
