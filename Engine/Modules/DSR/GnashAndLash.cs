using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using YapYapDraw.Engine.Interop;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Enum;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.DSR;

public class GnashAndLash : ISpecialAction
{
    private static readonly DrawElement Circle = new DrawElement
    {
        drawAvfx = "general_1bxf",
        radiusX = 8f,
        radiusZ = 8f,
        drawOnObject = true,
        hitCounter = new HitCounter
        {
            ActionID = new HashSet<uint> { 26389u }
        }
    };

    private static readonly DrawElement Donut = new DrawElement
    {
        drawAvfx = "customDonut",
        radiusX = 40f,
        radiusZ = 40f,
        refRadian = 0.2f,
        drawOnObject = true,
        refColor = GroundOmen.enemyColor,
        refTargetColor = GroundOmen.enemyColor,
        hitCounter = new HitCounter
        {
            ActionID = new HashSet<uint> { 26390u }
        }
    };

    public override string Name => "Gnash and Lash";

    public override uint Phase => 3u;

    public override HashSet<uint> ActionID => new HashSet<uint> { 26386u, 26389u };

    public override void OnActionCast(ActorCastInfo info)
    {
        IGameObject source = Svc.Objects.SearchById((ulong)info.SourceId);
        if (source != null && info.ActionId == 26386)
        {
            DrawQueue.Clear();
            DrawQueue.Enqueue((new HashSet<uint> { 26389u }, new(IGameObject, DrawElement[])[1] { (source, new DrawElement[1] { Donut }) }));
            DrawManager.Draw(Circle, source);
        }
    }
}
