using System;
using System.Collections.Generic;
using System.Linq;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;
using YapYapDraw.Engine.Vfx;

namespace YapYapDraw.Modules.Fight_92547;

public class HolyBlade : ISpecialAction
{
    public override string Name => "Holy Blade";

    public override HashSet<uint> ActionID => new HashSet<uint> { 43126u };

    public override IEnumerable<StaticVfx> ActiveAOEs => aoes.Take(2);

    public override void OnActionCast(ActorCastInfo info)
    {
        aoes.Add(SimpleElement.Fan(info.Pos, 24f, 120, info.Facing, Environment.TickCount64));
    }

    public override void OnAbilityCast(ActorAbilityInfo info)
    {
        if (aoes.Count > 0)
        {
            aoes[0].Remove();
            aoes.RemoveAt(0);
        }
    }
}
