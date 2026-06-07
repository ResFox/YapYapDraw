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

public class DarkSpineshatterDive : ISpecialAction
{
    public override string Name => "Dark Spineshatter Dive (tower)";

    public override uint Phase => 3u;

    public override HashSet<uint> ActionID => new HashSet<uint>();

    public override void OnAddStatus(ActorStatusChangeInfo info)
    {
        if (info.StatusID == 2756 && info.TargetID == ((IGameObject)Svc.Objects.LocalPlayer).GameObjectId)
        {
            DrawManager.Draw(new DrawElement
            {
                drawAvfx = "m0119_trap_02t",
                radiusX = 5f,
                radiusY = 5f,
                radiusZ = 5f,
                refOffsetZ = -15f,
                drawOnObject = true,
                delayDrawTime = (int)(info.Time - 3f) * 1000,
                hitCounter = new HitCounter
                {
                    ActionID = new HashSet<uint> { 26383u },
                    HitTarget = (IGameObject?)Svc.Objects.LocalPlayer
                }
            }, (IGameObject?)Svc.Objects.LocalPlayer);
            DrawManager.Draw(new DrawElement
            {
                drawType = ElementType.LockOn,
                drawAvfx = "m5fa_count5s_x",
                drawOnObject = true,
                delayDrawTime = (int)(info.Time - 5f) * 1000
            }, (IGameObject?)Svc.Objects.LocalPlayer);
        }
    }
}
