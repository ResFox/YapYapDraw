using System.Collections.Generic;
using System.Linq;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;
using YapYapDraw.Engine.Util;
using YapYapDraw.Engine.Vfx;

namespace YapYapDraw.Modules.M9S;

public class AetherDrain : ISpecialAction
{
    public override string Name => "Aether Drain";

    public override HashSet<uint> ActionID => new HashSet<uint> { 45970u, 45971u };

    public override IEnumerable<StaticVfx> ActiveAOEs => aoes.Take(4);

    public override void OnActionCast(ActorCastInfo info)
    {
        switch (info.ActionId)
        {
        case 45970:
            SimpleLockon.TarLockOn6m5s(info.TargetId.GameObject());
            break;
        case 45971:
            aoes.AddRange(SimpleElement.Cross(info.SourceId, 40f, 5f, info.Facing, info.CastTime * 1000f, 0f, 0u));
            break;
        }
    }

    public override void OnAbilityCast(ActorAbilityInfo info)
    {
        if (info.ActionId == 45971 && aoes.Count > 1)
        {
            aoes[0].Remove();
            aoes.RemoveAt(0);
            aoes[0].Remove();
            aoes.RemoveAt(0);
        }
    }
}
