using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using YapYapDraw.Engine.Interop;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.M10S;

public class BreakwaveSurge : ISpecialAction
{
    public override string Name => "Breakwave Surge";

    public override HashSet<uint> ActionID => new HashSet<uint> { 46540u, 46542u };

    public override void OnActionCast(ActorCastInfo info)
    {
        switch (info.ActionId)
        {
        case 46542:
            SimpleElement.Rectangle(info);
            break;
        case 46540:
            DrawManager.Draw(new DrawElement
            {
                drawAvfx = "e5d1_b1_kblaser_t1",
                radiusX = 1f,
                radiusZ = 10f,
                drawOnObject = true,
                hitCounter = new HitCounter
                {
                    ActionID = ActionID
                },
                KnockBackCheck = new KnockBackCheck
                {
                    Angle = info.Facing
                }
            }, (IGameObject?)Svc.Objects.LocalPlayer);
            break;
        }
    }
}
