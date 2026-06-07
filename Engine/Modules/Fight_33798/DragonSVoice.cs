using System.Collections.Generic;
using System.Numerics;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;
using YapYapDraw.Engine.Util;
using YapYapDraw.Engine.Vfx;

namespace YapYapDraw.Modules.Fight_33798;

public class DragonSVoice : ISpecialAction
{
    public override string Name => "Dragon's Voice";

    public override HashSet<uint> ActionID => new HashSet<uint> { 43832u, 43833u, 43839u, 43840u, 45056u, 45059u, 45072u, 45073u };

    public override void Update()
    {
        if (aoes.Count == 0)
        {
            return;
        }
        LockbladeDive specialAction = ModuleUtil.GetSpecialAction<LockbladeDive>();
        foreach (StaticVfx aoe in aoes)
        {
            aoe.Color = new Vector4(1f, 1f, 1f, (specialAction.aoes.Count > 0) ? 0.3f : YapYapDraw.Plugin.Config.CustomAlpha);
            aoe.TargetColor = new Vector4(1f, 1f, 1f, (specialAction.aoes.Count > 0) ? 0.3f : YapYapDraw.Plugin.Config.CustomAlpha);
        }
    }

    public override void OnActionCast(ActorCastInfo info)
    {
        aoes.Add(SimpleElement.Rectangle(info));
    }

    public override void OnAbilityCast(ActorAbilityInfo info)
    {
        if (aoes.Count > 0)
        {
            aoes.RemoveAt(0);
        }
    }
}
