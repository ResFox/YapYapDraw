using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.Interop;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;
using YapYapDraw.Engine.Util;

namespace YapYapDraw.Modules.DancingMad.P3;

public class ChaosWind : ISpecialAction
{
    public override string Name => "Chaos Wind";

    public override uint Phase => 3u;

    public override HashSet<uint> ActionID => new HashSet<uint> { 47862u };

    public override void OnAddStatus(ActorStatusChangeInfo info)
    {
        if (info.StatusID != 1602 && info.StatusID != 1603)
            return;

        IGameObject? lp = Svc.Objects.LocalPlayer;
        IGameObject? target = info.TargetID.GameObject();
        if (lp == null || target == null || info.TargetID != lp.GameObjectId)
            return;

        if (info.StatusID == 1602)
        {
            DrawElement element = new DrawElement
            {
                drawAvfx = "gl_fan090_1bpxf",
                radiusX = 15f,
                radiusZ = 15f,
                refRotation = 180.Degrees(),
                destroyTime = 10000f,
                delayDrawTime = (info.Time - 10f) * 1000f,
                StatusCheck = new StatusCheck
                {
                    CheckObject = target,
                    Status = 1602u
                },
                hitCounter = new HitCounter
                {
                    ActionID = new HashSet<uint> { 47891u }
                }
            };
            DrawManager.Draw(element, lp);
            element.drawAvfx = "gl_fan270_0100af";
            element.refRotation = 0.Degrees();
            DrawManager.Draw(element, lp);
        }
        else
        {
            DrawElement element = new DrawElement
            {
                drawAvfx = "gl_fan090_1bpxf",
                radiusX = 15f,
                radiusZ = 15f,
                destroyTime = 10000f,
                delayDrawTime = (info.Time - 10f) * 1000f,
                StatusCheck = new StatusCheck
                {
                    CheckObject = target,
                    Status = 1603u
                },
                hitCounter = new HitCounter
                {
                    ActionID = new HashSet<uint> { 47891u }
                }
            };
            DrawManager.Draw(element, lp);
            element.drawAvfx = "gl_fan270_0100af";
            element.refRotation = 180.Degrees();
            DrawManager.Draw(element, lp);
        }
    }
}
