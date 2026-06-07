using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.ClientState.Objects.Types;
using YapYapDraw.Engine.Interop;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.Managers;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;
using YapYapDraw.Engine.Util;

namespace YapYapDraw.Modules.GolbezEx;
public class Hypercharge : ISpecialAction
{
    public bool IsGathering;

    public override string Name => "Hypercharge";

    public override HashSet<uint> ActionID => new HashSet<uint> { 45663u, 45664u, 45670u, 45696u, 45679u };

    public override void OnActionCast(ActorCastInfo info)
    {
        switch (info.ActionId)
        {
        case 45663:
            IsGathering = true;
            break;
        case 45664:
            IsGathering = false;
            break;
        case 45679:
            SimpleElement.Rectangle(info);
            break;
        }
        ushort actionId = info.ActionId;
        if ((actionId != 45670 && actionId != 45696) || 1 == 0)
        {
            return;
        }
        DrawManager.Draw(new DrawElement
        {
            drawAvfx = "e5d1_b1_kblaser_t1",
            radiusX = 1f,
            radiusZ = 30f,
            drawOnObject = true,
            KnockBackCheck = new KnockBackCheck
            {
                Angle = ((info.ActionId == 45670) ? 0.Degrees() : (-180.Degrees()))
            },
            hitCounter = new HitCounter
            {
                ActionID = new HashSet<uint> { 45670u, 45696u }
            }
        }, (IGameObject?)Svc.Objects.LocalPlayer);
        if (IsGathering)
        {
            foreach (IGameObject player in PlayerHelper.AllPlayers)
            {
                SimpleLockon.TarLockOn5m5s(player, 6200f);
            }
            return;
        }
        foreach (IGameObject player in PlayerHelper.Healer.Union(PlayerHelper.Tank))
        {
            SimpleLockon.Share5S(player, 6200f);
        }
    }
}
