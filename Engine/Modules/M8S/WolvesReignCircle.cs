using System.Collections.Generic;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;
using YapYapDraw.Engine.Util;

namespace YapYapDraw.Modules.M8S;

public class WolvesReignCircle : ISpecialAction
{
    public override string Name => "Wolves' Reign(Circle)";

    public override HashSet<uint> ActionID => new HashSet<uint> { 43309u, 43310u, 43312u, 43313u };

    public override void OnActionCast(ActorCastInfo info)
    {
        switch (info.ActionId)
        {
        case 43309:
        case 43310:
            SimpleElement.Circle(info);
            break;
        case 43312:
        case 43313:
            DrawManager.Draw(new DrawElement
            {
                drawAvfx = "general_1bzt",
                radiusX = 6f,
                radiusZ = 6f,
                drawOnObject = true,
                hitCounter = new HitCounter
                {
                    ActionID = new HashSet<uint> { info.ActionId }
                }
            }, info.SourceId.GameObject());
            break;
        }
    }
}
