using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;
using YapYapDraw.Engine.Util;

namespace YapYapDraw.Modules.DSR;

public class BroadSwing : ISpecialAction
{
    private static readonly DrawElement Left = new DrawElement
    {
        drawAvfx = "gl_fan120_1bf",
        radiusX = 40f,
        radiusZ = 40f,
        drawOnObject = true,
        refRotation = 60.Degrees(),
        hitCounter = new HitCounter
        {
            ActionID = new HashSet<uint> { 25538u }
        }
    };

    private static readonly DrawElement Right = new DrawElement
    {
        drawAvfx = "gl_fan120_1bf",
        radiusX = 40f,
        radiusZ = 40f,
        drawOnObject = true,
        refRotation = -60.Degrees(),
        hitCounter = new HitCounter
        {
            ActionID = new HashSet<uint> { 25538u }
        }
    };

    private static readonly DrawElement Back = new DrawElement
    {
        drawAvfx = "gl_fan120_1bf",
        radiusX = 40f,
        radiusZ = 40f,
        drawOnObject = true,
        refRotation = 180.Degrees(),
        hitCounter = new HitCounter
        {
            ActionID = new HashSet<uint> { 25538u }
        }
    };

    public override string Name => "Broad Swing";

    public override uint Phase => 2u;

    public override HashSet<uint> ActionID => new HashSet<uint> { 25536u, 25537u, 25538u };

    public override void OnActionCast(ActorCastInfo info)
    {
        IGameObject source = info.SourceId.GameObject();
        if (source != null)
        {
            if (info.ActionId == 25536)
            {
                DrawManager.Draw(Right, source);
                DrawQueue.Clear();
                DrawQueue.Enqueue((new HashSet<uint> { 25538u }, new(IGameObject, DrawElement[])[1] { (source, new DrawElement[1] { Left }) }));
                DrawQueue.Enqueue((new HashSet<uint> { 25538u }, new(IGameObject, DrawElement[])[1] { (source, new DrawElement[1] { Back }) }));
            }
            if (info.ActionId == 25537)
            {
                DrawManager.Draw(Left, source);
                DrawQueue.Clear();
                DrawQueue.Enqueue((new HashSet<uint> { 25538u }, new(IGameObject, DrawElement[])[1] { (source, new DrawElement[1] { Right }) }));
                DrawQueue.Enqueue((new HashSet<uint> { 25538u }, new(IGameObject, DrawElement[])[1] { (source, new DrawElement[1] { Back }) }));
            }
        }
    }
}
