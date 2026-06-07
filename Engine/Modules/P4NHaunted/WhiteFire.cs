using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Util;

namespace YapYapDraw.Modules.P4NHaunted;

public class WhiteFire : ISpecialAction
{
    public override string Name => "White Fire";

    public override HashSet<uint> ActionID => new HashSet<uint>();

    public override void OnActorTetherEvent(uint actorId, uint Id, ulong targetId)
    {
        if (Id == 17)
        {
            DrawManager.Draw(new DrawElement
            {
                drawAvfx = "general02xf",
                radiusX = 2f,
                radiusZ = 100f,
                drawOnObject = true,
                target = targetId.GameObject(),
                hitCounter = new HitCounter
                {
                    ActionID = new HashSet<uint> { 33482u }
                }
            }, actorId.GameObject());
        }
    }
}
