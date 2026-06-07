using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.ClientState.Objects.Types;
using YapYapDraw.Engine.Interop;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Util;

namespace YapYapDraw.Modules.M10S;

public class LimitWave : ISpecialAction
{
    public override string Name => "Limit Wave";

    public override HashSet<uint> ActionID => new HashSet<uint>();

    public override void OnTargetIconEvent(IGameObject Source, uint icon, ulong TargetID)
    {
        if (icon - 635 <= 1)
        {
            DrawElement element = new DrawElement
            {
                drawAvfx = ((icon == 635) ? "general02pxf" : "general02xf"),
                drawOnObject = true,
                radiusX = 4f,
                radiusZ = 60f,
                target = TargetID.GameObject(),
                hitCounter = new HitCounter
                {
                    ActionID = new HashSet<uint> { 46545u, 46546u }
                }
            };
            IGameObject target = ((icon == 635) ? Svc.Objects.FirstOrDefault((IGameObject x) => x.BaseId == 19288) : Svc.Objects.FirstOrDefault((IGameObject x) => x.BaseId == 19287));
            DrawManager.Draw(element, target);
        }
    }

    public override void OnObjectCreatedEvent(IGameObject GameObject)
    {
        if (GameObject.BaseId == 19292)
        {
            DrawManager.Draw(new DrawElement
            {
                drawAvfx = "general_1bzt",
                radiusX = 4f,
                radiusZ = 4f,
                drawOnObject = true,
                OnlyVisible = true,
                hitCounter = new HitCounter
                {
                    ActionID = new HashSet<uint> { 46545u, 46546u },
                    TargetHitCount = 12
                }
            }, GameObject);
        }
    }
}
