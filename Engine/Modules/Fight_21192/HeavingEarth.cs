using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;
using YapYapDraw.Engine.Util;
using YapYapDraw.Engine.Vfx;

namespace YapYapDraw.Modules.Fight_21192;

public class HeavingEarth : ISpecialAction
{
    private readonly List<float> angles = new List<float>(4);

    private readonly WPos[] spiralSmallPoints = new WPos[4]
    {
        new WPos(34f, -710f),
        new WPos(31.8f, -715.5f),
        new WPos(35f, -721.7f),
        new WPos(41f, -722.5f)
    };

    private readonly WPos[] spiralBigPoints = new WPos[20]
    {
        new WPos(34f, -710f),
        new WPos(28.7f, -708.2f),
        new WPos(29.4f, -714f),
        new WPos(35.4f, -715.8f),
        new WPos(40f, -711f),
        new WPos(38.7f, -705f),
        new WPos(34f, -701.5f),
        new WPos(28f, -701.4f),
        new WPos(24f, -704.399f),
        new WPos(22f, -709.7f),
        new WPos(23.1f, -715.099f),
        new WPos(26.5f, -719.499f),
        new WPos(32f, -721.699f),
        new WPos(38f, -721.5f),
        new WPos(43f, -717.999f),
        new WPos(45.7f, -712.699f),
        new WPos(45.9f, -706.699f),
        new WPos(42.9f, -701.2f),
        new WPos(38.5f, -697f),
        new WPos(32.5f, -695.199f)
    };

    private int maxCasts;

    private readonly Angle[] anglesIntercardinals = new Angle[4]
    {
        -45.003f.Degrees(),
        44.998f.Degrees(),
        134.999f.Degrees(),
        -135.005f.Degrees()
    };

    public override string Name => "Heaving Earth";

    public override HashSet<uint> ActionID => new HashSet<uint> { 40661u, 40662u, 40606u };

    public override uint Phase => 3u;

    public override IEnumerable<StaticVfx> ActiveAOEs => aoes.Take(maxCasts);

    public override void OnActionCast(ActorCastInfo info)
    {
        if (info.ActionId == 40662)
        {
            angles.Add(info.Facing.Rad);
            if (angles.Count < 4)
            {
                return;
            }
            float angleSum = 0f;
            for (int i = 0; i < 4; i++)
            {
                angleSum += angles[i];
            }
            int roundedSum = (int)angleSum;
            if (roundedSum == -1 || roundedSum == 4)
            {
                for (int j = 0; j < 4; j++)
                {
                    AddAOEs(WPos.GenerateRotatedVertices(new WPos(34f, -710f), spiralSmallPoints, angles[j] * (180f / (float)Math.PI)));
                }
            }
            else if ((int)(2f * angleSum) == 3)
            {
                GenerateAOEsForMixedPattern(-45f, -135f);
            }
            else
            {
                GenerateAOEsForMixedPattern(45f, -45f);
            }
            angles.Clear();
            maxCasts = 16;
        }
        else if (info.ActionId == 40661)
        {
            WPos[] vertices = WPos.GenerateRotatedVertices(new WPos(34f, -710f), spiralBigPoints, info.Facing.Rad * (180f / (float)Math.PI));
            for (int k = 0; k < 20; k++)
            {
                aoes.Add(SimpleElement.Circle(new Vector3(vertices[k].X, -87.9f, vertices[k].Z), 5f, 4500 + 250 * k));
            }
            maxCasts = 10;
        }
    }

    public override void OnAbilityCast(ActorAbilityInfo info)
    {
        if (info.ActionId == 40606 && aoes.Count > 0)
        {
            aoes.RemoveAt(0);
        }
    }

    private void GenerateAOEsForMixedPattern(float intercardinalOffset, float cardinalOffset)
    {
        for (int i = 0; i < 4; i++)
        {
            float radians = angles[i];
            float degrees = radians * (180f / (float)Math.PI);
            bool isIntercardinal = false;
            for (int j = 0; j < 4; j++)
            {
                Angle angle = anglesIntercardinals[j];
                if (angle.AlmostEqual(new Angle(radians), (float)Math.PI / 180f))
                {
                    isIntercardinal = true;
                    break;
                }
            }
            float rotationAngle = (isIntercardinal ? (degrees + intercardinalOffset) : (degrees + cardinalOffset));
            AddAOEs(WPos.GenerateRotatedVertices(new WPos(34f, -710f), spiralSmallPoints, rotationAngle));
        }
    }

    private void AddAOEs(WPos[] points)
    {
        for (int i = 0; i < 4; i++)
        {
            aoes.Add(SimpleElement.Circle(new Vector3(points[i].X, -87.9f, points[i].Z), 5f, 6500 + 200 * i));
        }
    }
}
