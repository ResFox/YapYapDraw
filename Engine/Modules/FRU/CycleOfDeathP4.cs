using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;
using YapYapDraw.Engine.Util;

namespace YapYapDraw.Modules.FRU;

public class CycleOfDeathP4 : ISpecialAction
{
    public override string Name => "Cycle of Death";

    public override uint Phase => 4u;

    public override HashSet<uint> ActionID => new HashSet<uint> { 40247u, 40302u };

    public override void OnActionCast(ActorCastInfo info)
    {
        IGameObject? target = info.SourceId.GameObject()?.TargetObject;
        if (target != null)
        {
            SimpleLockon.ShareLockon(target);
        }
    }
}
