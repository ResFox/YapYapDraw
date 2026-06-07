using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.ClientState.Objects.Types;
using YapYapDraw.Engine.Interop;
using YapYapDraw.Engine.Interop.Game;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.TOP;

public class MonitorAoE : ISpecialAction
{
    public override string Name => "Monitor AoE";

    public override uint Phase => 3u;

    public override uint WeatherID => 79u;

    public override HashSet<uint> ActionID => new HashSet<uint> { 31595u, 31596u };

    public override void OnActionCast(ActorCastInfo info)
    {
        List<IGameObject> target = Svc.Objects.Where((IGameObject o) => (int)o.ObjectKind == 1 && o.GameObjectId != ((IGameObject)Player.Object).GameObjectId).ToList();
        DrawManager.Draw(new DrawElement
        {
            drawAvfx = "general_1bxf",
            radiusX = 7f,
            radiusZ = 7f,
            drawOnObject = true,
            hitCounter = new HitCounter
            {
                ActionID = new HashSet<uint> { 31597u }
            }
        }, target);
    }
}
