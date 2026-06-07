using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using YapYapDraw.Engine.Interop;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;

namespace YapYapDraw.Modules.TheMesoTerminal;

public class Impression : ISpecialAction
{
    public override string Name => "Impression";

    public override HashSet<uint> ActionID => new HashSet<uint> { 43819u };

    public override void OnActionCast(ActorCastInfo info)
    {
        DrawManager.Draw(new DrawElement
        {
            drawAvfx = "e5d1_b1_kblaser_t1",
            drawOnObject = true,
            radiusX = 1f,
            radiusZ = 11f,
            KnockBackCheck = new KnockBackCheck
            {
                OriginPos = info.Pos,
                Antiable = false
            },
            hitCounter = new HitCounter
            {
                ActionID = ActionID
            }
        }, (IGameObject?)Svc.Objects.LocalPlayer);
    }
}
