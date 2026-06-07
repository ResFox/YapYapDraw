using System;
using System.Collections.Generic;
using System.Numerics;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;
using YapYapDraw.Engine.Util;
using YapYapDraw.Engine.Vfx;

namespace YapYapDraw.Modules.M5;

public class RideTheWave : ISpecialAction
{
    private int exaflaresStarted;

    public override string Name => "Ride the Wave";

    public override HashSet<uint> ActionID => new HashSet<uint> { 42744u };

    public override void OnEnvControl(byte index, uint state)
    {
        if (index != 4)
        {
            return;
        }
        int[] columns = Array.Empty<int>();
        switch (state)
        {
        case 67109888u:
            columns = new int[7] { 0, 2, 3, 4, 5, 6, 7 };
            break;
        case 131074u:
        case 134219776u:
            columns = new int[7] { 0, 1, 3, 4, 5, 6, 7 };
            break;
        case 1048592u:
        case 268439552u:
            columns = new int[7] { 0, 1, 2, 4, 5, 6, 7 };
            break;
        case 2097184u:
        case 536879104u:
            columns = new int[7] { 0, 1, 2, 3, 5, 6, 7 };
            break;
        case 4194368u:
        case 1073758208u:
            columns = new int[7] { 0, 1, 2, 3, 4, 6, 7 };
            break;
        case 2147516416u:
            columns = new int[7] { 0, 1, 2, 3, 4, 5, 7 };
            break;
        case 524292u:
            exaflaresStarted++;
            break;
        }
        if (columns.Length != 0)
        {
            for (int i = 0; i < 7; i++)
            {
                List<StaticVfx> active = aoes;
                Vector3 pos = new Vector3(82.5f + (float)(columns[i] * 5), 0f, 70f);
                HitCounter hitCounter = new HitCounter
                {
                    ActionID = new HashSet<uint> { 42744u },
                    TargetHitCount = 15
                };
                active.Add(SimpleElement.Rectangle(pos, 15f, 2.5f, 0f, default, 3000f, 0f, hitCounter));
            }
        }
    }

    public override void OnAbilityCast(ActorAbilityInfo info)
    {
        int numCasts = base.NumCasts + 1;
        base.NumCasts = numCasts;
        if (base.NumCasts > 15)
        {
            aoes.ForEach(x =>
            {
                x.Remove();
            });
            aoes.Clear();
            return;
        }
        int count = ((exaflaresStarted == 1) ? 7 : 14);
        for (int i = 0; i < count; i++)
        {
            StaticVfx wave = aoes[i];
            Vector3 position = wave.Position;
            wave.Position = new Vector3(position.X, 0f, position.Z + 5f);
        }
    }

    public override void Reset()
    {
        exaflaresStarted = 0;
        base.Reset();
    }
}
