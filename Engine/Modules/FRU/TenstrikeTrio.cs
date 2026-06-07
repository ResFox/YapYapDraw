using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;
using YapYapDraw.Engine.Util;

namespace YapYapDraw.Modules.FRU;

public class TenstrikeTrio : ISpecialAction
{
    public override string Name => "Tenstrike Trio";

    public override uint Phase => 4u;

    public override HashSet<uint> ActionID => new HashSet<uint> { 40249u };

    public override void OnActionCast(ActorCastInfo info)
    {
        IGameObject? source = info.SourceId.GameObject();
        IGameObject target = ((source != null) ? source.TargetObject : null);
        if (target != null)
        {
            SimpleLockon.ShareLockon(target, 1000f);
        }
    }
}
