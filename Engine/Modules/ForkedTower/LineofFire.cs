using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.ForkedTower;

public class LineofFire : ISpecialAction
{
    public override string Name => "Line of Fire";

    public override HashSet<uint> ActionID => new HashSet<uint> { 42965u };

    public override uint Phase => 3u;

    public override void OnActionCast(ActorCastInfo info)
    {
        DrawManager.Draw(new DrawElement
        {
            drawAvfx = "general02xf",
            Position = info.Pos,
            drawOnObject = false,
            radiusX = 4f,
            radiusZ = 60f,
            refRotation = info.Facing,
            destroyTime = 9000f
        });
    }
}
