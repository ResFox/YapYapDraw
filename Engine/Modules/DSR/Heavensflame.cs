using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.ClientState.Objects.Types;
using YapYapDraw.Engine.Interop;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.DSR;

public class Heavensflame : ISpecialAction
{
    public override string Name => "Heavensflame";

    public override uint Phase => 1u;

    public override uint WeatherID => 2u;

    public override HashSet<uint> ActionID => new HashSet<uint> { 25310u };

    public override void OnActionCast(ActorCastInfo info)
    {
        List<IGameObject> target = Svc.Objects.Where((IGameObject obj) => (int)obj.ObjectKind == 1).ToList();
        DrawManager.Draw(new DrawElement
        {
            drawAvfx = "general_1bxf",
            radiusX = 10f,
            radiusY = 10f,
            radiusZ = 10f,
            drawOnObject = true,
            delayDrawTime = 4000f,
            hitCounter = new HitCounter
            {
                ActionID = new HashSet<uint> { 25311u }
            }
        }, target);
    }
}
