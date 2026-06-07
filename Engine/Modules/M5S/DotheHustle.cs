using System.Collections.Generic;
using System.Linq;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;
using YapYapDraw.Engine.Vfx;

namespace YapYapDraw.Modules.M5S;

public class DotheHustle : ISpecialAction
{
    public override string Name => "Do the Hustle";

    public override HashSet<uint> ActionID => new HashSet<uint> { 42869u, 42870u, 42789u, 42788u };

    public override IEnumerable<StaticVfx> ActiveAOEs => aoes.Take(2);

    public override void OnActionCast(ActorCastInfo info)
    {
        ushort actionId = info.ActionId;
        if ((uint)(actionId - 42869) <= 1u)
        {
            aoes.Add(SimpleElement.Fan(info.SourceId, 50f, 180, info.Facing, info.CastTime * 1000f, 0f, 0u));
        }
        else
        {
            SimpleElement.Fan(info, 180);
        }
    }

    public override void OnAbilityCast(ActorAbilityInfo info)
    {
        uint actionId = info.ActionId;
        bool isResolve = actionId - 42869 <= 1;
        if (isResolve && aoes.Count > 0)
        {
            aoes[0].Remove();
            aoes.RemoveAt(0);
        }
    }
}
