using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.P12S.P12S;

public class Glaukopis : ISpecialAction
{
    public override string Name => "Glaukopis";

    public override uint Phase => 1u;

    public override HashSet<uint> ActionID => new HashSet<uint> { 33532u };

    public override void OnActionCast(ActorCastInfo info)
    {
        SimpleElement.RectangleToTarget(info, 60f, 2.5f);
    }

    public override void OnAbilityCast(ActorAbilityInfo info)
    {
        DrawManager.Draw(new DrawElement
        {
            drawAvfx = "general02xf",
            radiusX = 2.5f,
            radiusZ = 60f,
            drawOnObject = true,
            target = info.Source.TargetObject,
            hitCounter = new HitCounter
            {
                ActionID = new HashSet<uint> { 33533u }
            }
        }, info.Source);
    }
}
