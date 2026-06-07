using System;
using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;
using YapYapDraw.Engine.Util;
using YapYapDraw.Engine.Vfx;

namespace YapYapDraw.Modules.CloudOfDarkness;

public class Phaser : ISpecialAction
{
    public override string Name => "Phaser (cone)";

    public override HashSet<uint> ActionID => new HashSet<uint> { 40497u };

    public override uint Phase => 3u;

    public override IEnumerable<StaticVfx> ActiveAOEs
    {
        get
        {
            int count = aoes.Count;
            if (count == 0)
            {
                return Array.Empty<StaticVfx>();
            }
            long drawTime = aoes[0].DrawTime;
            int i;
            for (i = 0; i < count && aoes[i].DrawTime - drawTime < 1000; i++)
            {
            }
            return aoes.Slice(0, i);
        }
    }

    public override void OnActionCast(ActorCastInfo info)
    {
        DrawElement element = new DrawElement
        {
            Enable = false,
            drawAvfx = "m0611_fan_60x",
            radiusX = 23f,
            radiusZ = 23f,
            drawOnObject = true,
            destroyTime = Environment.TickCount64
        };
        aoes.Add(DrawManager.Draw(element, info.SourceId.GameObject()));
    }

    public override void OnAbilityCast(ActorAbilityInfo info)
    {
        if (aoes.Count > 0)
        {
            aoes[0].Remove();
            aoes.RemoveAt(0);
        }
    }
}
