using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;
using YapYapDraw.Engine.Util;

namespace YapYapDraw.Modules.M9S;

public class BloodHungryClaw : ISpecialAction
{
    public override string Name => "Blood-Hungry Claw";

    public override HashSet<uint> ActionID => new HashSet<uint> { 45989u, 45991u };

    public override void OnAbilityCast(ActorAbilityInfo info)
    {
        base.NumCasts++;
        if (base.NumCasts >= 33)
        {
            if (base.NumCasts == 40)
            {
                base.NumCasts = 0;
            }
            return;
        }
        if (info.ActionId == 45989)
        {
            SimpleElement.Fan(info.Source, 30f, 30, info.Rotation + 20.Degrees(), 3000f, 0f, new HitCounter
            {
                ActionID = new HashSet<uint> { 45989u, 45991u },
                TargetHitCount = 8
            });
        }
        if (info.ActionId == 45991)
        {
            SimpleElement.Fan(info.Source, 30f, 30, info.Rotation + 20.Degrees(), 3000f, 0f, new HitCounter
            {
                ActionID = new HashSet<uint> { 45989u, 45991u },
                TargetHitCount = 8
            });
        }
    }
}
