using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.ClientState.Objects.Types;
using YapYapDraw.Engine.Interop;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Util;

namespace YapYapDraw.Modules.QueenEternalEx;
public class GravityRay : ISpecialAction
{
    public override string Name => "Gravity Ray";

    public override HashSet<uint> ActionID => new HashSet<uint>();

    public override void OnActorTetherEvent(uint actorId, uint Id, ulong targetId)
    {
        if (Id == 17)
        {
            IGameObject target = Svc.Objects.FirstOrDefault((IGameObject o) => o.BaseId == 18039);
            DrawManager.Draw(new DrawElement
            {
                drawAvfx = "m0611_fan_60x",
                radiusX = 50f,
                radiusZ = 50f,
                target = actorId.GameObject(),
                hitCounter = new HitCounter
                {
                    ActionID = new HashSet<uint> { 41005u }
                }
            }, target);
        }
    }
}
