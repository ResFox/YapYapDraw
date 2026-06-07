using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using YapYapDraw.Engine.Interop;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;
using YapYapDraw.Engine.Util;
using YapYapDraw.Engine.Vfx;

namespace YapYapDraw.Modules.E3;

public class NightRaidCommand : ISpecialAction
{
    public override string Name => "Night Raid Command";

    public override HashSet<uint> ActionID => new HashSet<uint> { 19490u, 19491u, 19516u, 19517u, 19518u, 19521u };

    public override void OnActionCast(ActorCastInfo info)
    {
        switch (info.ActionId)
        {
        case 19490:
        case 19516:
        case 19518:
            if (((IGameObject?)Svc.Objects.LocalPlayer).HasStatus(2238u))
            {
                List<StaticVfx> aoeList = aoes;
                uint sourceId = info.SourceId;
                Angle facing = info.Facing;
                float castTime = info.CastTime * 1000f;
                aoeList.Add(SimpleElement.Rectangle(sourceId, 50f, 5f, 0f, null, facing, castTime));
            }
            break;
        case 19491:
        case 19517:
        case 19521:
            if (((IGameObject?)Svc.Objects.LocalPlayer).HasStatus(2239u))
            {
                List<StaticVfx> aoeList = aoes;
                uint sourceId = info.SourceId;
                Angle facing = info.Facing;
                float castTime = info.CastTime * 1000f;
                aoeList.Add(SimpleElement.Rectangle(sourceId, 50f, 5f, 0f, null, facing, castTime));
            }
            break;
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
