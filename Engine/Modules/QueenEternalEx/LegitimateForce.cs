using System.Collections.Generic;
using System.Linq;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;
using YapYapDraw.Engine.Util;
using YapYapDraw.Engine.Vfx;

namespace YapYapDraw.Modules.QueenEternalEx;
public class LegitimateForce : ISpecialAction
{
    public override string Name => "Legitimate Force";

    public override HashSet<uint> ActionID => new HashSet<uint> { 40990u, 40992u, 40993u, 40994u };

    public override IEnumerable<StaticVfx> ActiveAOEs => aoes.Take(1);

    public override void OnActionCast(ActorCastInfo info)
    {
        switch (info.ActionId)
        {
        case 40990:
        {
            DrawElement drawElement = new DrawElement
            {
                drawAvfx = "general02wf",
                radiusX = 15f,
                radiusZ = 60f,
                refOffsetX = 15f,
                hitCounter = new HitCounter
                {
                    ActionID = new HashSet<uint> { 40990u }
                }
            };
            aoes.Add(DrawManager.Draw(drawElement, info.SourceId.GameObject()));
            drawElement.refOffsetX = -15f;
            drawElement.hitCounter = new HitCounter
            {
                ActionID = new HashSet<uint> { 40994u }
            };
            aoes.Add(DrawManager.Draw(drawElement, info.SourceId.GameObject()));
            break;
        }
        case 40992:
        {
            DrawElement drawElement = new DrawElement
            {
                drawAvfx = "general02wf",
                radiusX = 15f,
                radiusZ = 60f,
                refOffsetX = -15f,
                hitCounter = new HitCounter
                {
                    ActionID = new HashSet<uint> { 40992u }
                }
            };
            aoes.Add(DrawManager.Draw(drawElement, info.SourceId.GameObject()));
            drawElement.refOffsetX = 15f;
            drawElement.hitCounter = new HitCounter
            {
                ActionID = new HashSet<uint> { 40993u }
            };
            aoes.Add(DrawManager.Draw(drawElement, info.SourceId.GameObject()));
            break;
        }
        }
    }

    public override void OnAbilityCast(ActorAbilityInfo info)
    {
        if (aoes.Count > 0)
        {
            aoes.RemoveAt(0);
        }
    }
}
