using System;
using System.Collections.Generic;
using System.Numerics;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;
using YapYapDraw.Engine.Util;

namespace YapYapDraw.Modules.Everkeep;

public class ForgedTrack : ISpecialAction
{
    public enum Pattern
    {
        Unknown,
        A,
        B
    }

    private Pattern _patternN;

    private Pattern _patternS;

    public override string Name => "Forged Track";

    public override uint Phase => 2u;

    public override HashSet<uint> ActionID => new HashSet<uint> { 37761u, 37788u, 37789u, 37763u, 37766u };

    public override void OnActionCast(ActorCastInfo info)
    {
        if (info.ActionId != 37788)
        {
            return;
        }
        Vector3 offset = info.SourceId.GameObject().Position - new Vector3(100f, 0f, 100f);
        Vector2 axis = new Vector2(MathF.Sin(info.Facing.Rad), MathF.Cos(info.Facing.Rad));
        axis = new Vector2(0f - axis.Y, axis.X);
        float proj = offset.X * axis.X + offset.Z * axis.Y;
        bool positiveSide = proj > 0f;
        bool nearCenter = proj > -5f && proj < 5f;
        bool eastSide = offset.X > 0f;
        if (offset.Z < 0f)
        {
            if (_patternN != Pattern.Unknown)
            {
                bool aligned = eastSide == (_patternN == Pattern.A);
                float dist = proj + (float)(positiveSide ? (-5) : 5);
                if (aligned == positiveSide)
                {
                    SimpleElement.Rectangle(new Vector3(100f + axis.X * dist, 0f, 100f + axis.Y * dist), 10f, 7.5f, 10f, info.Facing, 3000f, 0f, new HitCounter
                    {
                        ActionID = new HashSet<uint> { 37763u }
                    });
                }
                else
                {
                    SimpleElement.RectangleKnockBack(new Vector3(100f + axis.X * dist, 0f, 100f + axis.Y * dist), 10f, 2.5f, 10f, info.Facing, 3000f, 0f, new HitCounter
                    {
                        ActionID = new HashSet<uint> { 37766u }
                    });
                }
            }
        }
        else if (_patternS != Pattern.Unknown)
        {
            bool flip = (eastSide == (_patternS == Pattern.A) == nearCenter) ^ positiveSide;
            float dist = (nearCenter ? 7.5f : 2.5f) * (float)(flip ? 1 : (-1));
            SimpleElement.Rectangle(new Vector3(100f + axis.X * dist, 0f, 100f + axis.Y * dist), 10f, 2.5f, 10f, info.Facing, 3000f, 0f, new HitCounter
            {
                ActionID = new HashSet<uint> { 37789u }
            });
        }
    }

    public override void OnEnvControl(byte index, uint state)
    {
        if (base.CanDraw)
        {
            switch (index)
            {
            case 2:
                AssignPattern(ref _patternS, 8388672u, 33554688u, state);
                break;
            case 3:
                AssignPattern(ref _patternS, 33554688u, 8388672u, state);
                break;
            case 5:
                AssignPattern(ref _patternN, 131073u, 2097168u, state);
                break;
            case 8:
                AssignPattern(ref _patternN, 2097168u, 131073u, state);
                break;
            case 4:
            case 6:
            case 7:
                break;
            }
        }
    }

    private static void AssignPattern(ref Pattern field, uint stateA, uint stateB, uint state)
    {
        if (state != 524292 && (state == stateA || state == stateB))
        {
            Pattern pattern = ((state == stateA) ? Pattern.A : Pattern.B);
            field = pattern;
        }
    }

    public override void OnAbilityCast(ActorAbilityInfo info)
    {
        if (info.ActionId == 37761)
        {
            base.CanDraw = true;
        }
        uint actionId = info.ActionId;
        if (actionId == 37763 || actionId == 37766 || actionId == 37789)
        {
            base.CanDraw = false;
            _patternN = Pattern.Unknown;
            _patternS = Pattern.Unknown;
        }
    }

    public override void Reset()
    {
        _patternN = Pattern.Unknown;
        _patternS = Pattern.Unknown;
        base.Reset();
    }
}
