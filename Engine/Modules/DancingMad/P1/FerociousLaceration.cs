using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.ClientState.Objects.Types;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;
using YapYapDraw.Engine.Util;

namespace YapYapDraw.Modules.DancingMad.P1;

public class FerociousLaceration : ISpecialAction
{
    private ulong _sourceId;

    public override string Name => "Ferocious Laceration";

    public override HashSet<uint> ActionID => new HashSet<uint> { 50179u };

    public override void OnActionCast(ActorCastInfo info)
    {
        _sourceId = info.SourceId;
        IGameObject? source = info.SourceId.GameObject();
        IGameObject? target = info.TargetId.GameObject();
        HitCounter hitCounter = new HitCounter
        {
            ActionID = new HashSet<uint> { 50179u }
        };
        SimpleElement.FanToTarget(source, target, 100f, 120, Follow: true, default, 0f, 3000f, hitCounter);
    }

    public override void OnAbilityCast(ActorAbilityInfo info)
    {
        IGameObject? target = PlayerHelper.RaidByEnmity(_sourceId).Skip(1).FirstOrDefault();
        IGameObject source = info.Source;
        HitCounter hitCounter = new HitCounter
        {
            ActionID = new HashSet<uint> { 50401u }
        };
        SimpleElement.FanToTarget(source, target, 100f, 120, Follow: true, default, 0f, 3000f, hitCounter);
    }
}
