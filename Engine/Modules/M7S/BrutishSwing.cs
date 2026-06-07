using System.Collections.Generic;
using System.Numerics;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;
using YapYapDraw.Engine.Util;

namespace YapYapDraw.Modules.M7S;

public class BrutishSwing : ISpecialAction
{
    public override string Name => "Brutish Swing";

    public override HashSet<uint> ActionID => new HashSet<uint> { 42337u, 42403u, 42386u, 42338u, 42387u, 42405u };

    public override void Update()
    {
        if (aoes.Count != 0)
        {
            SporeCloud mech = ModuleUtil.GetSpecialAction<SporeCloud>();
            TendrilsOfTerror tendrils = ModuleUtil.GetSpecialAction<TendrilsOfTerror>();
            bool dim = mech.aoes.Count > 0 || tendrils.aoes.Count > 0;
            aoes[0].Color = new Vector4(1f, 1f, 1f, dim ? 0.3f : YapYapDraw.Plugin.Config.CustomAlpha);
            aoes[0].TargetColor = new Vector4(1f, 1f, 1f, dim ? 0.3f : YapYapDraw.Plugin.Config.CustomAlpha);
        }
    }

    public override void OnActionCast(ActorCastInfo info)
    {
        switch (info.ActionId)
        {
        case 42337:
            SimpleElement.Circle(info);
            break;
        case 42386:
        case 42403:
            SimpleElement.Fan(info);
            break;
        case 42338:
        case 42387:
        case 42405:
            aoes.Add(SimpleElement.Donut(info));
            break;
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
