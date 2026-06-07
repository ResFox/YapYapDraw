using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Game.ClientState.Objects.Types;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;
using YapYapDraw.Engine.Util;
using YapYapDraw.Engine.Vfx;

namespace YapYapDraw.Modules.Fight_21192;

public class RockImpact : ISpecialAction
{
    private readonly WPos[] clockPositions = new WPos[4]
    {
        new WPos(34f, -697f),
        new WPos(48f, -710f),
        new WPos(21f, -710f),
        new WPos(34f, -724f)
    };

    private readonly Angle[] anglesCardinals = new Angle[4]
    {
        -90.004f.Degrees(),
        -0.003f.Degrees(),
        180.Degrees(),
        89.999f.Degrees()
    };

    public override string Name => "Rock Impact";

    public override HashSet<uint> ActionID => new HashSet<uint> { 40611u };

    public override uint Phase => 3u;

    public override IEnumerable<StaticVfx> ActiveAOEs => aoes.Take(8);

    public override void OnActionCast(ActorCastInfo info)
    {
        if (aoes.Count == 0)
        {
            float stepAngle = (float)(DetermineClockwise(info.SourceId.GameObject(), info.Facing) ? 1 : (-1)) * 22.5f;
            for (int i = 0; i < 15; i++)
            {
                WPos wPos = WPos.RotateAroundOrigin(stepAngle * (float)i, new WPos(34f, -710f), new WPos(info.SourceId.GameObject().Position));
                aoes.Add(SimpleElement.Circle(new Vector3(wPos.X, -87.9f, wPos.Z), 5f, 3000f, 0f, new HitCounter
                {
                    ActionID = new HashSet<uint> { 40611u },
                    TargetHitCount = i + 1
                }));
            }
        }
    }

    public override void OnAbilityCast(ActorAbilityInfo info)
    {
        if (aoes.Count != 0)
        {
            aoes.RemoveAt(0);
        }
    }

    private bool DetermineClockwise(IGameObject source, Angle rotation)
    {
        for (int i = 0; i < 4; i++)
        {
            if (new WPos(source.Position).AlmostEqual(clockPositions[i], 1f))
            {
                return rotation.AlmostEqual(anglesCardinals[i], (float)Math.PI / 180f);
            }
        }
        return false;
    }
}
