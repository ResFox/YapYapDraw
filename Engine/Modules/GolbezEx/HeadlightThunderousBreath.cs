using System.Collections.Generic;
using System.Linq;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;
using YapYapDraw.Engine.Util;
using YapYapDraw.Engine.Vfx;

namespace YapYapDraw.Modules.GolbezEx;
public class HeadlightThunderousBreath : ISpecialAction
{
    public override string Name => "Headlight / Thunderous Breath";

    public override HashSet<uint> ActionID => new HashSet<uint> { 45687u, 45689u, 45690u, 45692u };

    public override IEnumerable<StaticVfx> ActiveAOEs => aoes.Take(1);

    public override void OnActionCast(ActorCastInfo info)
    {
        DrawElement drawElement = new DrawElement
        {
            drawAvfx = "mdl_general03_o0e1",
            drawOnObject = true,
            radiusX = 35f,
            radiusZ = 70f,
            refOffsetY = ((info.ActionId != 45687) ? (-5) : 0),
            hitCounter = new HitCounter
            {
                ActionID = new HashSet<uint> { 45689u, 45692u },
                TargetHitCount = 2
            }
        };
        aoes.Add(DrawManager.Draw(drawElement, info.SourceId.GameObject()));
        drawElement.refOffsetY = ((info.ActionId == 45687) ? (-5) : 0);
        aoes.Add(DrawManager.Draw(drawElement, info.SourceId.GameObject()));
    }

    public override void OnAbilityCast(ActorAbilityInfo info)
    {
        uint actionId = info.ActionId;
        bool isHit = actionId == 45689 || actionId == 45692;
        if (isHit && aoes.Count > 0)
        {
            aoes[0].Remove();
            aoes.RemoveAt(0);
        }
    }
}
