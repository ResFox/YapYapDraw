using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.Fight_56456;

public class HeavyMemory : ISpecialAction
{
    public override string Name => "Heavy Memory";

    public override HashSet<uint> ActionID => new HashSet<uint> { 37144u };

    public override uint Phase => 3u;

    public override void OnAbilityCast(ActorAbilityInfo info)
    {
        DrawManager.Draw(new DrawElement
        {
            drawAvfx = "general02pxf",
            radiusX = 4f,
            radiusZ = 80f,
            target = info.Target,
            drawOnObject = true,
            hitCounter = new HitCounter
            {
                ActionID = new HashSet<uint> { 37143u }
            }
        }, info.Source);
    }
}
