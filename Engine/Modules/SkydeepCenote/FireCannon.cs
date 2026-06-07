using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Enum;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;
using YapYapDraw.Engine.Util;

namespace YapYapDraw.Modules.SkydeepCenote;

public class FireCannon : ISpecialAction
{
    public override string Name => "Fire Cannon";

    public override uint Phase => 2u;

    public override HashSet<uint> ActionID => new HashSet<uint> { 38660u, 38661u, 38662u, 38663u };

    public override void OnActionCast(ActorCastInfo info)
    {
        DrawManager.Draw(new DrawElement
        {
            drawAvfx = "customRect2",
            radiusX = 5f,
            radiusZ = 5f,
            drawOnObject = true,
            hitCounter = new HitCounter
            {
                ActionID = new HashSet<uint> { info.ActionId }
            },
            refColor = GroundOmen.Yellow,
            refTargetColor = GroundOmen.Yellow
        }, info.SourceId.GameObject());
    }
}
