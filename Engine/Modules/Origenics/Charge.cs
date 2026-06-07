using System.Collections.Generic;
using System.Numerics;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;
using YapYapDraw.Engine.Util;

namespace YapYapDraw.Modules.Origenics;

public class Charge : ISpecialAction
{
    public override string Name => "Charge";

    public override uint Phase => 3u;

    public override HashSet<uint> ActionID => new HashSet<uint> { 38953u, 38954u };

    public override void OnActionCast(ActorCastInfo info)
    {
        if (info.ActionId == 38953)
        {
            base.NumCasts++;
            DrawManager.Draw(new DrawElement
            {
                drawAvfx = "general02xf",
                radiusX = 5f,
                drawOnObject = true,
                targetPosition = new Vector3(info.Pos.X, info.Pos.Y, info.Pos.Z),
                endToTarget = true,
                hitCounter = new HitCounter
                {
                    ActionID = new HashSet<uint> { 38954u, 36431u },
                    TargetHitCount = base.NumCasts
                }
            }, info.SourceId.GameObject());
        }
    }

    public override void OnAbilityCast(ActorAbilityInfo info)
    {
        if (info.ActionId == 38954)
        {
            base.NumCasts = 0;
        }
    }
}
