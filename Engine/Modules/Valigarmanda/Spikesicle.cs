using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;
using YapYapDraw.Engine.Util;

namespace YapYapDraw.Modules.Valigarmanda;

public class Spikesicle : ISpecialAction
{
    private List<int> shapeIndexHit;

    public override string Name => "Spikesicle";

    public override uint Phase => 1u;

    public override HashSet<uint> ActionID => new HashSet<uint> { 36850u };

    public override void OnEnvControl(byte index, uint state)
    {
        if (state == 131076 && index >= 4 && index <= 13)
        {
            int shapeIndex = index - 4 >> 1;
            bool isOdd = (index & 1) != 0;
            float xOffset = ((index < 12) ? ((float)(isOdd ? (-20) : 20)) : (isOdd ? 17.5f : (-17.5f)));
            float castDelay = ((base.NumCasts == 0) ? 0f : ((9.3f + 0.2f * (float)base.NumCasts) * 1000f));
            switch (shapeIndex)
            {
            case 0:
                shapeIndexHit[0]++;
                SimpleElement.Donut(new Vector3(100f + xOffset, 0f, 85f), 20f, 25f, 3000f, castDelay, new HitCounter
                {
                    ActionID = new HashSet<uint> { 36853u },
                    TargetHitCount = shapeIndexHit[0]
                });
                break;
            case 1:
                shapeIndexHit[1]++;
                SimpleElement.Donut(new Vector3(100f + xOffset, 0f, 85f), 25f, 30f, 3000f, castDelay, new HitCounter
                {
                    ActionID = new HashSet<uint> { 36854u },
                    TargetHitCount = shapeIndexHit[1]
                });
                break;
            case 2:
                shapeIndexHit[2]++;
                SimpleElement.Donut(new Vector3(100f + xOffset, 0f, 85f), 30f, 35f, 3000f, castDelay, new HitCounter
                {
                    ActionID = new HashSet<uint> { 36855u },
                    TargetHitCount = shapeIndexHit[2]
                });
                break;
            case 3:
                shapeIndexHit[3]++;
                SimpleElement.Donut(new Vector3(100f + xOffset, 0f, 85f), 35f, 40f, 3000f, castDelay, new HitCounter
                {
                    ActionID = new HashSet<uint> { 36856u },
                    TargetHitCount = shapeIndexHit[3]
                });
                break;
            case 4:
            {
                shapeIndexHit[4]++;
                Vector3 pos = new Vector3(100f + xOffset, 0f, 85f);
                float delay = castDelay;
                HitCounter hitCounter = new HitCounter
                {
                    ActionID = new HashSet<uint> { 36857u },
                    TargetHitCount = shapeIndexHit[4]
                };
                SimpleElement.Rectangle(pos, 40f, 2.5f, 0f, default, 3000f, delay, hitCounter);
                break;
            }
            }
            base.NumCasts++;
        }
    }

    public override void OnAbilityCast(ActorAbilityInfo info)
    {
        int count = 5;
        List<int> list = new List<int>(count);
        CollectionsMarshal.SetCount(list, count);
        Span<int> span = CollectionsMarshal.AsSpan(list);
        span[0] = 0;
        span[1] = 0;
        span[2] = 0;
        span[3] = 0;
        span[4] = 0;
        shapeIndexHit = list;
        base.NumCasts = 0;
    }

    public override void Reset()
    {
        int count = 5;
        List<int> list = new List<int>(count);
        CollectionsMarshal.SetCount(list, count);
        Span<int> span = CollectionsMarshal.AsSpan(list);
        span[0] = 0;
        span[1] = 0;
        span[2] = 0;
        span[3] = 0;
        span[4] = 0;
        shapeIndexHit = list;
        base.Reset();
    }
}
