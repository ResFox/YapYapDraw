using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.ClientState.Statuses;
using YapYapDraw.Engine.Interop;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;
using YapYapDraw.Engine.Util;
using YapYapDraw.Engine.Vfx;

namespace YapYapDraw.Modules.E3;

public class StormCommand : ISpecialAction
{
    public override string Name => "Storm Command";

    public override HashSet<uint> ActionID => new HashSet<uint> { 19516u, 19518u, 20067u };

    public override IEnumerable<StaticVfx> ActiveAOEs => aoes.Take(2);

    public override void OnActionCast(ActorCastInfo info)
    {
        if (!((IBattleChara)Svc.Objects.LocalPlayer).StatusList.Any(status =>
        {
            uint statusId = status.StatusId;
            return statusId - 2238 <= 1;
        }))
        {
            switch (info.ActionId)
            {
            case 19516:
            case 19518:
            {
                List<StaticVfx> aoeList = aoes;
                uint sourceId = info.SourceId;
                Angle facing = info.Facing;
                float castTime = info.CastTime * 1000f;
                aoeList.Add(SimpleElement.Rectangle(sourceId, 50f, 5f, 0f, null, facing, castTime));
                break;
            }
            case 20067:
            {
                List<StaticVfx> aoeList = aoes;
                uint sourceId = info.SourceId;
                Angle facing = info.Facing;
                float castTime = info.CastTime * 1000f;
                aoeList.Add(SimpleElement.Rectangle(sourceId, 25f, 5f, 0f, null, facing, castTime));
                break;
            }
            }
        }
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
