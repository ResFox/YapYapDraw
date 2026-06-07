using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;
using YapYapDraw.Engine.Util;

namespace YapYapDraw.Modules.M6S;

public class WaterIII : ISpecialAction
{
    public override string Name => "Water III";

    public override uint Phase => 2u;

    public override HashSet<uint> ActionID => new HashSet<uint> { 37831u };

    public override void OnActionCast(ActorCastInfo info)
    {
        IGameObject? source = info.SourceId.GameObject();
        SimpleElement.Circle((source != null) ? source.TargetObject : null, 8f, 3200f);
    }
}
