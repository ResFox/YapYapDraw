using System.Collections.Generic;
using System.Linq;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;
using YapYapDraw.Engine.Util;
using YapYapDraw.Engine.Vfx;

namespace YapYapDraw.Modules.M1S;

public class One_twoPaw : ISpecialAction
{
    public override string Name => "One-two Paw (clone)";

    public override uint Phase => 1u;

    public override HashSet<uint> ActionID => new HashSet<uint> { 37989u, 37990u, 37993u, 37992u };

    public override IEnumerable<StaticVfx> ActiveAOEs => aoes.Take(2);

    public override void OnActionCast(ActorCastInfo info)
    {
        DrawElement element = new DrawElement
        {
            drawAvfx = "gl_fan180_1bf",
            radiusX = 100f,
            radiusZ = 100f,
            drawOnObject = true,
            refRotation = info.Facing,
            fixRotation = true,
            destroyTime = info.CastTime * 1000f,
            hitCounter = new HitCounter
            {
                ActionID = new HashSet<uint> { info.ActionId }
            }
        };
        aoes.Add(DrawManager.Draw(element, info.SourceId.GameObject()));
        aoes.SortBy((StaticVfx x) => x.DrawTime);
    }

    public override void OnAbilityCast(ActorAbilityInfo info)
    {
        if (aoes.Count > 0)
        {
            aoes.RemoveAt(0);
        }
    }
}
