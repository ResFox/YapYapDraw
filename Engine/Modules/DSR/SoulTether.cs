using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.ClientState.Objects.Types;
using YapYapDraw.Engine.Interop;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.ModuleSetup;

namespace YapYapDraw.Modules.DSR;

public class SoulTether : ISpecialAction
{
    public override string Name => "Soul Tether";

    public override uint Phase => 3u;

    public override uint WeatherID => 66u;

    public override HashSet<uint> ActionID => new HashSet<uint>();

    public override void OnActorTetherEvent(uint actorId, uint Id, ulong targetId)
    {
        if (Id == 84 && base.NumCasts <= 0)
        {
            base.NumCasts++;
            DrawElement element = new DrawElement
            {
                drawAvfx = "general_1bxf",
                radiusX = 5f,
                radiusZ = 5f,
                drawOnObject = true,
                TetherCheck = new TetherCheck
                {
                    CheckType = 1,
                    TetherID = new HashSet<int> { 84 }
                },
                hitCounter = new HitCounter
                {
                    ActionID = new HashSet<uint> { 26396u }
                }
            };
            List<IGameObject> target = Svc.Objects.Where((IGameObject o) => (int)o.ObjectKind == 1).ToList();
            DrawManager.Draw(element, target);
        }
    }
}
