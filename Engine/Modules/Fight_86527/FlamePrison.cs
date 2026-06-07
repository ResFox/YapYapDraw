using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using YapYapDraw.Engine.Interop;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Enum;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.Fight_86527;

public class FlamePrison : ISpecialAction
{
    public override string Name => "Flame Prison";

    public override HashSet<uint> ActionID => new HashSet<uint> { 44107u, 44100u };

    public override void OnActionCast(ActorCastInfo info)
    {
        DrawManager.Draw(new DrawElement
        {
            drawType = ElementType.LockOn,
            drawAvfx = "m0656_stop_s5count3x",
            delayDrawTime = ((info.ActionId == 44107) ? 2000 : 0)
        }, (IGameObject?)Svc.Objects.LocalPlayer);
    }
}
