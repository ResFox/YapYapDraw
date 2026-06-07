using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.ClientState.Objects.Types;
using YapYapDraw.Engine.Interop;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.ModuleSetup;

namespace YapYapDraw.Modules.M8S;

public class StalkingStoneWind : ISpecialAction
{
    public override string Name => "Stalking Stone/Wind";

    public override HashSet<uint> ActionID => new HashSet<uint>();

    public override uint WeatherID => 2u;

    public override void OnTargetIconEvent(IGameObject target, uint icon, ulong TargetID)
    {
        if (icon == 23)
        {
            IGameObject addA = Svc.Objects.FirstOrDefault((IGameObject o) => o.BaseId == 18225);
            IGameObject addB = Svc.Objects.FirstOrDefault((IGameObject o) => o.BaseId == 18219);
            IGameObject add = (target.HasStatus(4389u) ? addA : addB);
            DrawManager.Draw(new DrawElement
            {
                drawAvfx = "general02pxf",
                Position = add.Position,
                drawOnObject = false,
                radiusX = 3f,
                radiusZ = 40f,
                target = target,
                hitCounter = new HitCounter
                {
                    ActionID = new HashSet<uint> { 41935u, 41956u }
                }
            });
            SimpleLockon.ShareRect5s(target, add);
        }
    }
}
