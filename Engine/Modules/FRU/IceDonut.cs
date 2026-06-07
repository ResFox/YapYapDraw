using System;
using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.ClientState.Objects.Types;
using YapYapDraw.Engine.Interop;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;
using YapYapDraw.Engine.Util;
using YapYapDraw.Engine.Vfx;

namespace YapYapDraw.Modules.FRU;

public class IceDonut : ISpecialAction
{
    public override string Name => "Ice (donut)";

    public override uint Phase => 3u;

    public override HashSet<uint> ActionID => new HashSet<uint> { 40279u };

    public override IEnumerable<StaticVfx> ActiveAOEs
    {
        get
        {
            if (aoes.Count == 0 || Svc.Objects.LocalPlayer == null)
            {
                return Array.Empty<StaticVfx>();
            }
            StaticVfx nearest = aoes.OrderBy((StaticVfx aoe) => aoe.Owner.DistanceSquaredToTarget((IGameObject)Svc.Objects.LocalPlayer)).FirstOrDefault();
            if (nearest == null)
            {
                return Array.Empty<StaticVfx>();
            }
            return new[] { nearest };
        }
    }

    public override void OnAddStatus(ActorStatusChangeInfo info)
    {
        if (info.StatusID == 2462)
        {
            aoes.Add(SimpleElement.Donut(info.TargetID.GameObject(), 3f, 12f, 5000f, (info.Time - 5f) * 1000f));
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
