using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;
using YapYapDraw.Engine.Util;

namespace YapYapDraw.Modules.TOP;

public class MonitorCleave : ISpecialAction
{
    public override string Name => "Monitor Cleave";

    public override uint Phase => 5u;

    public override uint WeatherID => 174u;

    public override HashSet<uint> ActionID => new HashSet<uint> { 31638u, 31639u };

    public override void OnActionCast(ActorCastInfo info)
    {
        IGameObject source = info.SourceId.GameObject();
        SimpleElement.Fan(source, 100f, 180, source.Rotation.Radians() + ((info.ActionId == 31638) ? (-90.Degrees()) : 90.Degrees()), 3000f, 0f, new HitCounter
        {
            ActionID = new HashSet<uint> { info.ActionId }
        });
    }
}
