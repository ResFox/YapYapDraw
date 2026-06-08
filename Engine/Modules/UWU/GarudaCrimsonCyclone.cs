using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Util;

namespace YapYapDraw.Modules.UWU;

public class GarudaCrimsonCyclone : ISpecialAction
{
    public override string Name => "Garuda Crimson Cyclone";

    public override uint Phase => 1u;

    public override HashSet<uint> ActionID => new HashSet<uint> { 11103u };

    public override uint WeatherID => 28u;

    public override void OnActorPlayActionTimelineEvent(IGameObject source, uint id)
    {
        if (base.NumCasts == 0 && source.BaseId == 8730 && id == 7747)
        {
            base.NumCasts++;
            Angle rotation = source.Rotation.Radians();
            HitCounter hitCounter = new HitCounter
            {
                ActionID = new HashSet<uint> { 11103u }
            };
            SimpleElement.Rectangle(source, 49f, 9f, 5f, null, rotation, 3000f, 0f, hitCounter);
        }
    }
}
