using System.Collections.Generic;
using System.Numerics;
using Dalamud.Game.ClientState.Objects.Types;
using YapYapDraw.Engine.Interop;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.TenderValley;

public class BarrelSmashKnockback : ISpecialAction
{
    public override string Name => "Barrel Smash (knockback)";

    public override HashSet<uint> ActionID => new HashSet<uint> { 37390u };

    public override uint Phase => 1u;

    public override void OnActionCast(ActorCastInfo info)
    {
        DrawManager.Draw(new DrawElement
        {
            drawAvfx = "e5d1_b1_kblaser_t1",
            radiusX = 1f,
            radiusZ = 20f,
            drawOnObject = true,
            KnockBackCheck = new KnockBackCheck
            {
                OriginPos = new Vector3(info.Pos.X, -4f, info.Pos.Z)
            },
            hitCounter = new HitCounter
            {
                ActionID = new HashSet<uint> { 37390u }
            }
        }, (IGameObject?)Svc.Objects.LocalPlayer);
    }
}
