using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.UWU;

public class RockThrow : ISpecialAction
{
    public override string Name => "Rock Throw";

    public override uint Phase => 3u;

    public override HashSet<uint> ActionID => new HashSet<uint> { 11115u, 11116u };

    public override uint WeatherID => 29u;

    public override void OnAbilityCast(ActorAbilityInfo info)
    {
        DrawManager.Draw(new DrawElement
        {
            drawAvfx = "general_1bpxf",
            radiusX = 6f,
            radiusZ = 6f,
            drawOnObject = true,
            destroyTime = 5000f
        }, info.Target);
    }
}
