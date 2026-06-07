using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.ClientState.Objects.Types;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;
using YapYapDraw.Engine.Util;

namespace YapYapDraw.Modules.M10S;

public class CrossDualSpiral : ISpecialAction
{
    public override string Name => "Cross / Dual Spiral Water";

    public override HashSet<uint> ActionID => new HashSet<uint> { 46560u, 46561u, 46557u, 46558u };

    public static bool onlyBuff => PlayerHelper.AllPlayers.Any((IGameObject x) => x.HasStatus(4975u));

    public override void OnActionCast(ActorCastInfo info)
    {
        if (onlyBuff)
        {
            foreach (IGameObject item in PlayerHelper.AllPlayers.Where((IGameObject x) => x.HasStatus(4975u)))
            {
                IGameObject? source = info.SourceId.GameObject();
                IGameObject? target = item.GameObjectId.GameObject();
                HitCounter hitCounter = new HitCounter
                {
                    ActionID = new HashSet<uint> { 46561u, 46558u }
                };
                SimpleElement.FanToTarget(source, target, 60f, 30, Follow: true, default, 0f, 3000f, hitCounter);
            }
            return;
        }
        foreach (IGameObject allPlayer in PlayerHelper.AllPlayers)
        {
            IGameObject? source2 = info.SourceId.GameObject();
            IGameObject? target2 = allPlayer.GameObjectId.GameObject();
            HitCounter hitCounter = new HitCounter
            {
                ActionID = new HashSet<uint> { 46561u, 46558u }
            };
            SimpleElement.FanToTarget(source2, target2, 60f, 30, Follow: true, default, 0f, 3000f, hitCounter);
        }
    }

    public override void OnAbilityCast(ActorAbilityInfo info)
    {
        if (info.ActionId == 46561)
        {
            SimpleElement.Fan(info.Source, 60f, 15, info.Rotation + 22.5f.Degrees(), 3000f, 0f, new HitCounter
            {
                ActionID = new HashSet<uint> { 46562u }
            });
            SimpleElement.Fan(info.Source, 60f, 15, info.Rotation - 22.5f.Degrees(), 3000f, 0f, new HitCounter
            {
                ActionID = new HashSet<uint> { 46562u }
            });
        }
        if (info.ActionId == 46558)
        {
            SimpleElement.Fan(info.Source, 60f, 30, info.Rotation, 3000f, 0f, new HitCounter
            {
                ActionID = new HashSet<uint> { 46559u }
            });
        }
    }
}
