using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;
using YapYapDraw.Engine.Util;

namespace YapYapDraw.Modules.M3S;

public class MurderousMist : ISpecialAction
{
    public override string Name => "Murderous Mist";

    public override uint Phase => 1u;

    public override HashSet<uint> ActionID => new HashSet<uint> { 37886u, 39895u };

    public override void OnActionCast(ActorCastInfo info)
    {
        switch (info.ActionId)
        {
        case 37886:
            SimpleElement.Fan(info, 40f, 270);
            break;
        case 39895:
            DrawManager.Draw(new DrawElement
            {
                drawAvfx = "gl_fan090_1bf",
                radiusX = 40f,
                radiusZ = 40f,
                refRotation = info.Facing + 180.Degrees(),
                fixRotation = true,
                drawOnObject = true,
                hitCounter = new HitCounter
                {
                    ActionID = new HashSet<uint> { 39895u }
                }
            }, info.SourceId.GameObject());
            break;
        }
    }
}
