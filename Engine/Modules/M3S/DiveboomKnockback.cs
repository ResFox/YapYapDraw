using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using YapYapDraw.Engine.Interop;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;
using YapYapDraw.Engine.Util;

namespace YapYapDraw.Modules.M3S;

public class DiveboomKnockback : ISpecialAction
{
    public override string Name => "Diveboom (knockback)";

    public override uint Phase => 1u;

    public override HashSet<uint> ActionID => new HashSet<uint> { 37869u, 37878u };

    public override void OnActionCast(ActorCastInfo info)
    {
        DrawManager.Draw(new DrawElement
        {
            drawAvfx = "e5d1_b1_kblaser_t1",
            radiusX = 1f,
            radiusZ = 25f,
            drawOnObject = true,
            KnockBackCheck = new KnockBackCheck
            {
                OriginPos = info.SourceId.GameObject().Position
            },
            hitCounter = new HitCounter
            {
                ActionID = ActionID
            }
        }, (IGameObject?)Svc.Objects.LocalPlayer);
    }
}
