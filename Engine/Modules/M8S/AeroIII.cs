using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using YapYapDraw.Engine.Interop;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.M8S;

public class AeroIII : ISpecialAction
{
    public override string Name => "AeroIII";

    public override HashSet<uint> ActionID => new HashSet<uint> { 41912u };

    public override void OnActionCast(ActorCastInfo info)
    {
        DrawManager.Draw(new DrawElement
        {
            drawAvfx = "e5d1_b1_kblaser_t1",
            radiusX = 1f,
            radiusZ = 8f,
            drawOnObject = true,
            KnockBackCheck = new KnockBackCheck
            {
                OriginPos = info.Pos
            },
            hitCounter = new HitCounter
            {
                ActionID = new HashSet<uint> { info.ActionId }
            }
        }, (IGameObject?)Svc.Objects.LocalPlayer);
    }
}
