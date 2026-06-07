using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using YapYapDraw.Engine.Interop;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;
using YapYapDraw.Engine.Util;

namespace YapYapDraw.Modules.UWU;

public class GreatWrathKnockback : ISpecialAction
{
    public override string Name => "Great Wrath (knockback)";

    public override uint Phase => 3u;

    public override HashSet<uint> ActionID => new HashSet<uint> { 11111u };

    public override void OnActionCast(ActorCastInfo info)
    {
        DrawManager.Draw(new DrawElement
        {
            drawAvfx = "e5d1_b1_kblaser_t1",
            radiusX = 1f,
            radiusZ = 24f,
            drawOnObject = true,
            destroyTime = 4000f,
            KnockBackCheck = new KnockBackCheck
            {
                OriginPos = info.SourceId.GameObject().Position,
                Antiable = false
            }
        }, (IGameObject?)Svc.Objects.LocalPlayer);
    }
}
