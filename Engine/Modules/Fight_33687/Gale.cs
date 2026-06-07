using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using YapYapDraw.Engine.Interop;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.Fight_33687;

public class Gale : ISpecialAction
{
    public override string Name => "Gale";

    public override HashSet<uint> ActionID => new HashSet<uint> { 36612u };

    public override void OnActionCast(ActorCastInfo info)
    {
        DrawManager.Draw(new DrawElement
        {
            drawAvfx = "e5d1_b1_kblaser_t1",
            radiusX = 1f,
            radiusZ = 20f,
            drawOnObject = true,
            refRotation = info.Facing,
            fixRotation = true,
            hitCounter = new HitCounter
            {
                ActionID = new HashSet<uint> { 36612u }
            }
        }, (IGameObject?)Svc.Objects.LocalPlayer);
    }
}
