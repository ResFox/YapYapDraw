using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using YapYapDraw.Engine.Interop;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;
using YapYapDraw.Engine.Util;

namespace YapYapDraw.Modules.Fight_33687;

public class Downburst : ISpecialAction
{
    public override string Name => "Downburst";

    public override HashSet<uint> ActionID => new HashSet<uint> { 36610u };

    public override void OnActionCast(ActorCastInfo info)
    {
        DrawManager.Draw(new DrawElement
        {
            drawAvfx = "e5d1_b1_kblaser_t1",
            radiusX = 1f,
            radiusZ = 10f,
            drawOnObject = true,
            hitCounter = new HitCounter
            {
                ActionID = new HashSet<uint> { 36610u }
            },
            KnockBackCheck = new KnockBackCheck
            {
                OriginPos = info.SourceId.GameObject().Position
            }
        }, (IGameObject?)Svc.Objects.LocalPlayer);
    }
}
