using System.Collections.Generic;
using System.Numerics;
using Dalamud.Game.ClientState.Objects.Types;
using YapYapDraw.Engine.Interop;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.M1S;

public class MouserKnockback : ISpecialAction
{
    public override string Name => "Mouser (knockback)";

    public override uint Phase => 2u;

    public override HashSet<uint> ActionID => new HashSet<uint> { 37964u };

    public override void OnActionCast(ActorCastInfo info)
    {
        DrawManager.Draw(new DrawElement
        {
            drawAvfx = "e5d1_b1_kblaser_t1",
            radiusX = 1f,
            radiusZ = 21f,
            drawOnObject = true,
            KnockBackCheck = new KnockBackCheck
            {
                OriginPos = new Vector3(100f, 0f, 100f),
                Antiable = false
            },
            hitCounter = new HitCounter
            {
                ActionID = new HashSet<uint> { 37964u }
            }
        }, (IGameObject?)Svc.Objects.LocalPlayer);
    }
}
