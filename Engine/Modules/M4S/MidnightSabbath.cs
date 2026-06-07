using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.ClientState.Objects.Types;
using FFXIVClientStructs.FFXIV.Client.UI;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;
using YapYapDraw.Engine.Util;

namespace YapYapDraw.Modules.M4S;

public class MidnightSabbath : ISpecialAction
{
    private enum Shape
    {
        None,
        Line,
        Donut
    }

    public override string Name => "Midnight Sabbath";

    public override uint Phase => 6u;

    public override HashSet<uint> ActionID => new HashSet<uint> { 38444u, 38443u };

    public override void OnActionCast(ActorCastInfo info)
    {
        if (info.ActionId == 38444)
        {
            SimpleElement.ShowText("Spread first, then stack", (TextGimmickHintStyle)0, 10);
            foreach (IGameObject allPlayer in PlayerHelper.AllPlayers)
            {
                SimpleLockon.TarLockOn5m5s(allPlayer, 2000f);
            }
            {
                foreach (IGameObject member in PlayerHelper.Tank.Union(PlayerHelper.Healer))
                {
                    SimpleLockon.ShareLockon2(member, 6100f);
                }
                return;
            }
        }
        SimpleElement.ShowText("Stack first, then spread", (TextGimmickHintStyle)0, 10);
        foreach (IGameObject member in PlayerHelper.Tank.Union(PlayerHelper.Healer))
        {
            SimpleLockon.ShareLockon2(member, 2000f);
        }
        foreach (IGameObject allPlayer2 in PlayerHelper.AllPlayers)
        {
            SimpleLockon.TarLockOn5m5s(allPlayer2, 6100f);
        }
    }

    public override void OnActorPlayActionTimelineEvent(IGameObject source, uint id)
    {
        if (source.BaseId == 17323)
        {
            var (shape, startDelay, half) = id switch
            {
                4561u => (Shape.Line, 0, true), 
                4562u => (Shape.Line, 8100, false), 
                4563u => (Shape.Donut, 0, true), 
                4564u => (Shape.Donut, 8100, false), 
                _ => default((Shape, int, bool)), 
            };
            switch (shape)
            {
            case Shape.Line:
            {
                Angle rotation = source.Rotation.Radians();
                float delay = startDelay;
                HitCounter hitCounter = new HitCounter
                {
                    ActionID = new HashSet<uint> { 38436u, 38439u },
                    TargetHitCount = (half ? 4 : 8)
                };
                SimpleElement.Rectangle(source, 40f, 5f, 0f, null, rotation, 3000f, delay, hitCounter);
                break;
            }
            default:
                SimpleElement.Donut(source, 5f, 15f, 3000f, startDelay, new HitCounter
                {
                    ActionID = new HashSet<uint> { 38436u, 38439u },
                    TargetHitCount = (half ? 4 : 8)
                });
                break;
            case Shape.None:
                break;
            }
        }
    }
}
