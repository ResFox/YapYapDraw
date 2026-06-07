using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.M6S;

public class ColorClash : ISpecialAction
{
    private bool? partnerStack;

    public override string Name => "Color Clash";

    public override HashSet<uint> ActionID => new HashSet<uint> { 42635u, 42637u };

    public override void OnActionCast(ActorCastInfo info)
    {
        partnerStack = info.ActionId == 42637;
    }

    public override void OnAddStatus(ActorStatusChangeInfo info)
    {
        if (info.StatusID != 4163)
        {
            return;
        }
        bool? choice = partnerStack;
        if (!choice.HasValue)
        {
            return;
        }
        if (choice == true)
        {
            foreach (IGameObject dps in PlayerHelper.DPS)
            {
                SimpleLockon.ShareLockon2(dps);
            }
        }
        else
        {
            foreach (IGameObject healer in PlayerHelper.Healer)
            {
                SimpleLockon.ShareLockon(healer);
            }
        }
        partnerStack = null;
    }

    public override void Reset()
    {
        base.Reset();
        partnerStack = null;
    }
}
