using System.Collections.Generic;
using System.Linq;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;
using YapYapDraw.Engine.Util;
using YapYapDraw.Engine.Vfx;

namespace YapYapDraw.Modules.M6S;

public class SprayPain : ISpecialAction
{
    public override string Name => "Spray Pain";

    public override uint Phase => 2u;

    public override HashSet<uint> ActionID => new HashSet<uint> { 42657u, 39468u };

    public override IEnumerable<StaticVfx> ActiveAOEs => aoes.Take(5);

    public override void OnActionCast(ActorCastInfo info)
    {
        if (info.ActionId == 42657)
        {
            aoes.Add(SimpleElement.Circle(info.SourceId, 10f, 7000f, 0f, 0u, null));
            return;
        }
        DrawManager.Draw(new DrawElement
        {
            drawAvfx = "m0347_sircle_01m1",
            radiusX = 10f,
            radiusZ = 10f,
            drawOnObject = true,
            destroyTime = 8500f
        }, info.SourceId.GameObject());
    }

    public override void OnAbilityCast(ActorAbilityInfo info)
    {
        if (info.ActionId == 42657 && aoes.Count > 0)
        {
            aoes.RemoveAt(0);
        }
    }
}
