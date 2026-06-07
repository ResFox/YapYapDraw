using System.Collections.Generic;
using System.Numerics;
using Dalamud.Game.ClientState.Objects.Types;
using YapYapDraw.Engine.Interop;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;
using YapYapDraw.Engine.Util;

namespace YapYapDraw.Modules.FRU;

public class LightRampantReturn : ISpecialAction
{
    private readonly List<Vector3> posMap = new List<Vector3>();

    public override string Name => "Light Rampant (return)";

    public override uint Phase => 4u;

    public override HashSet<uint> ActionID => new HashSet<uint> { 40251u };

    public override void OnActionCast(ActorCastInfo info)
    {
        posMap.Add(info.SourceId.GameObject().Position);
        if (posMap.Count == 2)
        {
            Vector3 midpoint = (posMap[0] + posMap[1]) / 2f;
            DrawElement trapElement = new DrawElement
            {
                drawAvfx = "m0119_trap_02t",
                Position = new Vector3(midpoint.X, 0f, midpoint.Y),
                drawOnObject = false,
                radiusX = 2f,
                radiusY = 5f,
                radiusZ = 2f,
                StatusCheck = new StatusCheck
                {
                    CheckObject = (IGameObject)Svc.Objects.LocalPlayer,
                    Status = 4208u
                },
                hitCounter = new HitCounter
                {
                    ActionID = new HashSet<uint> { 40332u }
                }
            };
            aoes.Add(DrawManager.Draw(trapElement, (IGameObject?)Svc.Objects.LocalPlayer));
            DrawElement laserElement = new DrawElement
            {
                drawAvfx = "e5d1_b1_kblaser_t1",
                radiusX = 1f,
                drawOnObject = true,
                targetPosition = new Vector3(midpoint.X, 0f, midpoint.Y),
                StatusCheck = new StatusCheck
                {
                    CheckObject = (IGameObject)Svc.Objects.LocalPlayer,
                    Status = 4208u
                },
                hitCounter = new HitCounter
                {
                    ActionID = new HashSet<uint> { 40332u }
                }
            };
            aoes.Add(DrawManager.Draw(laserElement, (IGameObject?)Svc.Objects.LocalPlayer));
        }
    }

    public override void Reset()
    {
        posMap.Clear();
        base.Reset();
    }
}
